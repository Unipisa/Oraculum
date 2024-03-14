import { Fact } from './../../interfaces/fact';
import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { Message } from 'src/app/interfaces/message';
import { PredictService } from 'src/app/services/predict.service';
import { StreamService } from 'src/app/services/stream.service';
import { DomSanitizer } from '@angular/platform-browser';
import { v4 as uuidv4 } from 'uuid';
import { MatDialog } from '@angular/material/dialog';
import { DialogFeedbackComponent } from '../dialogs/dialog-feedback/dialog-feedback.component';
import { ConfigService } from 'src/app/services/config.service';
import { FactDialogComponent } from '../dialogs/fact-dialog/fact-dialog.component';
import { AuthService } from 'src/app/services/auth.service';
import { BotInfo } from 'src/app/interfaces/botInfo';
import { Chat } from 'src/app/interfaces/chat';
import { KnowledgeService } from 'src/app/services/knowledge.service';
import { QueryPayload_for_facts } from 'src/app/services/knowledge.service';
import { SpeechRecognitionModalComponent } from '../speech-recognition-modal/speech-recognition-modal.component';
import { SnackbarService } from 'src/app/services/snackbar.service';
import language from 'src/app/config/language';

@Component({
  selector: 'app-chatExplain',
  templateUrl: './chatExplain.component.html',
  styleUrls: ['./chatExplain.component.scss'],
})
export class ChatExplainComponent implements OnInit {
  language = language;
  botInfo: BotInfo = this.authService.selectedBot;
  @ViewChild('messagesContainer') private messagesContainer!: ElementRef;
  thumbUpSelected: boolean = false;
  defaultBaseAssistantPrompt: string = language.defaultBaseAssistantPrompt;
  messages: Message[] = [
    {
      id: uuidv4(),
      text: this.botInfo
        ? this.botInfo.baseAssistantPrompt
        : this.defaultBaseAssistantPrompt,
      sender: 'assistant',
      timestamp: new Date().toLocaleTimeString().slice(0, 5),
    },
  ];
  facts!: Fact[];
  activeChat!: Chat;
  // N_factsUsedForAnswer = this.configService.contentsForResponse.getValue();
  // N_totalFacts = this.configService.totalContentsHome.getValue();
  N_factsUsedForAnswer!: number;
  N_totalFacts!: number;
  userMessageId!: string;
  assistantMessageId!: string;
  chatInput: string = '';
  loading: boolean = false;
  loadingText: string = '';
  loadingChats: boolean = false;
  buttonDisabled = false;
  oneshot: boolean = true;
  results: any;
  constructor(
    private predictService: PredictService,
    private streamService: StreamService,
    private sanitizer: DomSanitizer,
    public dialogFeedback: MatDialog,
    public configService: ConfigService,
    private dialog: MatDialog,
    private authService: AuthService,
    private knowledgeService: KnowledgeService,
    private snackbarService: SnackbarService
  ) {}

  ngOnInit() {
    this.newChat();
  }

  linkify(text: string) {
    const urlRegex =
      /(\b(https?|ftp|file):\/\/[-A-Z0-9+&@#\/%?=~_|!:,.;]*[-A-Z0-9+&@#\/%=~_|])/gi;
    const replacedText = text.replace(urlRegex, (url) => {
      return '<a href="' + url + '" target="_blank">' + url + '</a>';
    });
    return replacedText;
  }

  openRecognitionModal() {
    const dialogRef = this.dialogFeedback.open(
      SpeechRecognitionModalComponent,
      {
        disableClose: true,
      }
    );

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        //console.log('The dialog was closed with result:', result);
        // Process the result here
        //console.log('Result:', result);
        this.sendMessage(result);
      }
    });
  }

  sendMessage(text?: string) {
    // console.log('send message');
    if (
      (this.chatInput != '' &&
        this.chatInput != null &&
        this.chatInput != undefined) ||
      (text != '' && text != null && text != undefined)
    ) {
      this.messages.push({
        id: uuidv4(),
        text: text ? text : this.chatInput,
        sender: 'user',
        timestamp: new Date().toLocaleTimeString().slice(0, 5),
        completed: false,
      });
      this.chatInput = '';
      this.loading = true;
      const question = this.messages
        .filter((m) => m.sender === 'user')
        .filter((m) => m.completed !== true)
        .map((m) => m.text)
        .pop();
      this.loading = true;
      this.predictService
        .askExplainNew(question, this.activeChat.sibyllaId, this.activeChat.id)
        .subscribe({
          next: (res) => {
            this.userMessageId = res.userMessageId;
            this.assistantMessageId = res.assistantMessageId;
            this.messages.push({
              id: uuidv4(),
              text: res.answer,
              sender: 'assistant',
              timestamp: new Date().toLocaleTimeString().slice(0, 5),
              completed: true,
            });
            this.N_factsUsedForAnswer = res.usedFactsList.length;
            this.N_totalFacts =
              res.usedFactsList.length + res.extraFactsList.length;
            const factsList: Fact[] = [
              ...res.usedFactsList.map((fact: Fact) => ({
                ...fact,
                score: Math.round((1 - fact.distance!) * 100),
                outOfLimit: false,
              })),
              ...res.extraFactsList.map((fact: Fact) => ({
                ...fact,
                score: Math.round((1 - fact.distance!) * 100),
                outOfLimit: true,
              })),
            ];

            this.facts = factsList.sort((a, b) => b.score! - a.score!);
            //console.log(this.facts);

            this.loading = false;
            this.oneshot = false;
          },
        });
      // this.setAllMessagesCompleted()
    }
    // console.log(this.messages);
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  private scrollToBottom(): void {
    try {
      this.messagesContainer.nativeElement.scrollTo({
        top: this.messagesContainer.nativeElement.scrollHeight,
        behavior: 'smooth',
      });
    } catch (err) {}
  }

  //keypress event enter
  onKey(event: any) {
    // console.log(event);
    if (event.key === 'Enter') {
      this.sendMessage();
    }
  }

  giveFeedbackMessage(messageId: string, newFeedback: boolean): void {
    const messageIndex = this.messages.findIndex((m) => m.id === messageId);
    if (messageIndex !== -1) {
      this.messages[messageIndex] = {
        ...this.messages[messageIndex],
        feedback: newFeedback,
      };
    }
    //console.log(this.messages[messageIndex]);
    this.openDialogFeedback(messageId);
  }

  openDialogFeedback(messageId: string) {
    const dialogRef = this.dialogFeedback.open(DialogFeedbackComponent, {
      minWidth: '50%',
    });
    dialogRef.afterClosed().subscribe((result) => {
      //console.log('The dialog was closed: ', result);
      if (result) {
        const messageIndex = this.messages.findIndex((m) => m.id === messageId);
        if (messageIndex !== -1) {
          this.messages[messageIndex] = {
            ...this.messages[messageIndex],
            feedbackText: result,
          };
        }
      }
    });
  }

  giveFeedBackFact(id: string, feedback: boolean) {
    const payload: QueryPayload_for_facts = {
      messageId: this.assistantMessageId,
      factId: id,
      questionMessageId: this.userMessageId,
      rating: feedback.toString(),
    };

    this.knowledgeService
      .postFeedbackFact(payload, this.botInfo.id, this.activeChat.id)
      .subscribe({
        next: (res) => {
          //console.log('feedback send');
          const factIndex = this.facts.findIndex((fact) => fact.id === id);
          if (factIndex !== -1) {
            // Aggiorna il feedback dell'elemento corrispondente
            this.facts[factIndex].feedback = feedback;
          }
        },
        error: (err) => {
          console.error('Error:', err);
        },
      });
  }

  copyContent(content: string): void {
    navigator.clipboard.writeText(content).then(
      () => {
        //console.log('Text copied successfully', content);
        // You can also implement a notification to the user here
      },
      (err) => {
        console.error('Error in copying text: ', err);
      }
    );
  }


  // set all messages completed
  setAllMessagesCompleted() {
    this.messages.forEach((m) => (m.completed = true));
  }

  openUrl(url: string) {
    window.open(url, '_blank');
  }
  openFactDialog(fact?: Fact) {
    this.knowledgeService.getSibyllaeConfigs().subscribe((response) => {
      const selectedSibylla = localStorage.getItem('selectedSibylla');
      const categoryFilter = response.find((c) => c.id == selectedSibylla)
        ?.memoryConfiguration.categoryFilter![0];
      const dialogRef = this.dialog.open(FactDialogComponent, {
        width: '500px',
        data: fact
          ? fact
          : { category: categoryFilter ? categoryFilter : selectedSibylla },
      });

      dialogRef.afterClosed().subscribe((result) => {
        //console.log('The dialog was closed');
        // Handle any actions after the dialog is closed
        // log the result
        //console.log(result);
        let fact!: Fact;
        // create new fact with result
        let formValue = result.formValue;
        if (formValue) {
          fact = {
            id: result.id ? result.id : '',
            title: formValue.title,
            factType: formValue.factType,
            category: formValue.category,
            expiration: formValue.expiration,
            tags: formValue.tags,
            content: formValue.content,
            citation: formValue.source,
            reference: '',
          };
        }
        if (result.action === 'add') {
          this.knowledgeService.postFact(fact).subscribe({
            next: (response) => {
              //console.log(response);
              this.results = response;
              this.snackbarService.showSnackbar(
                'Elemento aggiunto',
                'Chiudi',
                2000
              );
            },
            error: (err) => {
              console.error('Error:', err);
              this.snackbarService.showSnackbar('Errore', 'Chiudi', 2000);
            },
          });
        } else if (result.action === 'edit') {
          this.knowledgeService.updateFact(fact).subscribe({
            next: (response: any) => {
              //console.log(response);
              this.results = response;
              this.snackbarService.showSnackbar(
                'Elemento modificato',
                'Chiudi',
                2000
              );
            },
            error: (err: any) => {
              console.error('Error:', err);
              this.snackbarService.showSnackbar('Errore', 'Chiudi', 2000);
            },
          });
        } else if (result.action === 'delete') {
          this.knowledgeService.deleteFact(result.id).subscribe({
            next: (response: any) => {
              //console.log(response);
              this.results = response;
              this.snackbarService.showSnackbar(
                'Elemento eliminato',
                'Chiudi',
                2000
              );
            },
            error: (err: any) => {
              console.error('Error:', err);
              this.snackbarService.showSnackbar('Errore', 'Chiudi', 2000);
            },
          });
        }
      });
    });
  }
  newChat() {
    if (this.buttonDisabled) return;
    this.loading = true;
    this.buttonDisabled = true;
    this.facts = [];
    this.N_factsUsedForAnswer = 0;
    this.N_totalFacts = 0;
    this.authService.newChat('explain').subscribe({
      next: (c: Chat) => {
        this.activeChat = c;
        this.messages = [
          {
            id: uuidv4(),
            text: this.botInfo
              ? this.botInfo.baseAssistantPrompt
              : this.defaultBaseAssistantPrompt,
            sender: 'assistant',
            timestamp: new Date().toLocaleTimeString().slice(0, 5),
          },
        ];
      },
      error: (err) => {
        console.error('Error:', err);
      },
      complete: () => {
        this.loading = false;
        setTimeout(() => (this.buttonDisabled = false), 5000);
      },
    });
  }
}
