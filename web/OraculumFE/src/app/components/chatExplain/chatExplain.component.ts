import { Fact } from './../../interfaces/fact';
import { Component, OnInit, ElementRef, ViewChild, Inject } from '@angular/core';
import { Message } from 'src/app/interfaces/message';
import { PredictService } from 'src/app/services/predict.service';
import { StreamService } from 'src/app/services/stream.service';
import { DomSanitizer } from '@angular/platform-browser';
import { v4 as uuidv4 } from 'uuid';
import { MatDialog } from '@angular/material/dialog';
import { DialogFeedbackComponent } from '../dialogs/dialog-feedback/dialog-feedback.component';
import { ConfigService } from 'src/app/services/config.service';
import { AuthService } from 'src/app/services/auth.service';
import { BotInfo } from 'src/app/interfaces/botInfo';
import { Chat } from 'src/app/interfaces/chat';
import { KnowledgeService } from 'src/app/services/knowledge.service';
import { QueryPayload_for_facts } from 'src/app/services/knowledge.service';
import { SnackbarService } from 'src/app/services/snackbar.service';
import language from 'src/app/config/language';
import { AddSourceDialogComponent } from '../dialogs/add-source-dialog/add-source-dialog.component';
import { FactDialogComponent } from '../dialogs/fact-dialog/fact-dialog.component';

@Component({
  selector: 'app-chatExplain',
  templateUrl: './chatExplain.component.html',
  styleUrls: ['./chatExplain.component.scss'],
})
export class ChatExplainComponent implements OnInit {
  @ViewChild(AddSourceDialogComponent)
  addSourceDialog!: AddSourceDialogComponent;
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
  activeChatId!: string;
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

  ngOnInit() {}

  updateUserMessage(messageId: string){
    this.userMessageId = messageId;
  }

  updateAssistantMessage(messageId: string){
    this.assistantMessageId = messageId;
  }

  updateActiveChat(chatId: string) {
    this.activeChatId = chatId;
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
  handleFactsChange(facts: Fact[]) {
    console.log('Received updated facts:', facts);
    this.facts = facts;
  }

  buildFeedBackFact(fact: Fact, feedback: boolean) {
    console.log('buildFeedBackFact', fact, feedback)
    if (feedback == false) {
      const dialogRef = this.dialogFeedback.open(DialogFeedbackComponent, {
        minWidth: '50%',
      });
      dialogRef.afterClosed().subscribe((result) => {
        if (result) {
          this.giveFeedBackFact(fact.id, feedback, result);
        }
      });
    }else{
      this.giveFeedBackFact(fact.id, feedback);
    }
  }

  giveFeedBackFact(id: string, feedback: boolean, textFeedback?: string) {
    console.log('giveFeedBackFact', id, feedback, textFeedback)
    const payload: QueryPayload_for_facts = {
      messageId: this.assistantMessageId,
      factId: id,
      questionMessageId: this.userMessageId,
      rating: feedback.toString(),
      text: textFeedback
    };

    this.knowledgeService
      .postFeedbackFact(payload, this.botInfo.id, this.activeChatId)
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

  openAddSourceDialog(): void {
    const dialogRef = this.dialog.open(AddSourceDialogComponent, {
      minWidth: '50%',
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log('Dialog closed with result:', result);

      this.knowledgeService.getSibyllaeConfigs().subscribe((response) => {
        const selectedSibylla = localStorage.getItem('selectedSibylla');
        const categoryFilter = response.find((c) => c.id == selectedSibylla)
          ?.memoryConfiguration.categoryFilter![0];

      if (result.type === 'application/pdf' || result.type.includes('word')){
        this.knowledgeService.ingestDocument(result.content, categoryFilter!).subscribe({
        });
      }
      if (result.type === 'audio/mp3' || result.type === 'video/mp4'){
        this.knowledgeService.ingestAudioVideo(result.content,categoryFilter!).subscribe({
        });
      }
      if (result.type === 'url'){
        this.knowledgeService.ingestWebpage(result.content,categoryFilter!).subscribe({
        });
      }
      if (result.type === 'text'){
        let cont = result.content
        this.knowledgeService.ingestText(cont,categoryFilter!).subscribe({
        });
      }
      
    });
  });
  }

}
