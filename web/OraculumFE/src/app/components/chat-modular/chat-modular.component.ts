import { BotInfo } from './../../interfaces/botInfo';
import {
  Component,
  OnInit,
  ViewChild,
  ElementRef,
  Input,
  Output,
  EventEmitter,
} from '@angular/core';
import { Message } from 'src/app/interfaces/message';
import { PredictService } from 'src/app/services/predict.service';
import { StreamService } from 'src/app/services/stream.service';
import { DomSanitizer } from '@angular/platform-browser';
import { v4 as uuidv4 } from 'uuid';
import { MatDialog } from '@angular/material/dialog';
import { DialogFeedbackComponent } from '../dialogs/dialog-feedback/dialog-feedback.component';
import { AuthService } from 'src/app/services/auth.service';
import { Chat } from 'src/app/interfaces/chat';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { SpeechRecognitionModalComponent } from '../speech-recognition-modal/speech-recognition-modal.component';
import language from 'src/app/config/language';
import { Fact } from 'src/app/interfaces/fact';
import { ConfirmDialogComponent } from '../dialogs/confirm-newchat/confirm-newChat.component';
import { DeviceDetectorService } from 'ngx-device-detector';

@Component({
  selector: 'app-chat-modular',
  templateUrl: './chat-modular.component.html',
  styleUrls: ['./chat-modular.component.scss'],
})
export class ChatModularComponent implements OnInit {
  language = language;
  botInfo: BotInfo = this.authService.selectedBot;
  activeChat!: Chat;
  defaultBaseAssistantPrompt: string = language.defaultBaseAssistantPrompt;
  @Input() mode: 'normal' | 'explain' = 'normal';
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
  loadingChats: boolean = false;

  // array of random messages to select if the bot doesn't know the answer
  @ViewChild('messagesContainer') private messagesContainer!: ElementRef;
  chatInput: string = '';
  loading: boolean = false;
  loadingText: string = '';
  buttonDisabled = false;
  clearSession: boolean = true;
  facts!: Fact[];
  N_factsUsedForAnswer!: number;
  N_totalFacts!: number;
  @Output() factsChanged = new EventEmitter<Fact[]>();
  @Output() updateUserMessage = new EventEmitter<string>();
  @Output() updateAssistantMessage = new EventEmitter<string>();
  @Output() updateActiveChat = new EventEmitter<string>();
  text = '';
  isMobile: boolean = false;
  shouldScroll: boolean = false;
  errorRetryButton: boolean = false;

  constructor(
    private predictService: PredictService,
    private streamService: StreamService,
    private sanitizer: DomSanitizer,
    public dialogFeedback: MatDialog,
    private authService: AuthService,
    private oidcSecurityService: OidcSecurityService,
    private dialog: MatDialog,
    private deviceService: DeviceDetectorService
  ) {
    this.isMobile = this.deviceService.isMobile();
    console.log('isMobile:', this.isMobile);
  }

  ngOnInit() {
    this.executeChatCreation();
  }

  newChat() {
    if (this.buttonDisabled) return;
    // Apertura del dialogo di conferma
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      minWidth: '40%',
      minHeight: '20%',
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result === true) {
        this.errorRetryButton = false;
        this.executeChatCreation();
      } else {
        console.log('Chat creation cancelled!');
      }
    });
  }

  private executeChatCreation(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.loading = true;
      this.buttonDisabled = true;
      this.facts = [];
      this.factsChanged.emit(this.facts);

      this.authService.newChat(this.mode).subscribe({
        next: (c: Chat) => {
          this.activeChat = c;
          this.updateActiveChat.emit(this.activeChat.id);
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
          resolve();
        },
        error: (err: any) => {
          console.error('Error:', err);
          reject(err);
        },
        complete: () => {
          this.loading = false;
          setTimeout(() => {
            this.buttonDisabled = false;
            resolve();
          }, 5000);
        },
      });
    });
  }

  openRecognitionModal() {
    const dialogRef = this.dialogFeedback.open(
      SpeechRecognitionModalComponent,
      {
        disableClose: true,
      }
    );

    dialogRef.afterClosed().subscribe((result: string | undefined) => {
      if (result) {
        this.sendMessage(result);
      }
    });
  }

  linkify(message: Message): string {
    const urlRegex =
      /(\b(https?|ftp|file):\/\/[-A-Z0-9+&@#\/%?=~_|!:,.;]*[-A-Z0-9+&@#\/%=~_|])/gi;
    message.sources = []; // Initialize sources array
    let modifiedText = message.text.replace(urlRegex, (url: string) => {
      // Instead of just returning the anchor tag, also push the url to sources
      const urlObj = new URL(url);
      const display = urlObj.hostname + urlObj.pathname;
      message.sources!.push({ url: url, display: display });
      return ''; // Remove the URL from the message text
    });

    // Remove parentheses from the modified text
    modifiedText = modifiedText.replace(/\(|\)/g, '');
    // remove eventual spaces and . at the end of the text
    if (modifiedText.endsWith('.')) {
      modifiedText = modifiedText.slice(0, -1);
    }
    modifiedText = modifiedText.trim();

    return modifiedText;
  }

  sendMessage(text?: string) {
    if (!text && !this.chatInput) {
      return;
    }

    const messageData = {
      id: uuidv4(),
      text: text || this.chatInput,
      sender: 'user',
      timestamp: new Date().toLocaleTimeString().slice(0, 5),
      completed: false,
    };

    this.messages.push(messageData);
    this.chatInput = ''; // Clear the chat input
    this.loading = true;

    const question = this.messages
      .filter((m) => m.sender === 'user' && !m.completed)
      .map((m) => m.text)
      .pop();

    if (this.mode === 'explain') {
      this.predictService
        .askExplainNew(question, this.activeChat.sibyllaId, this.activeChat.id)
        .subscribe({
          next: (res) => {
            this.updateAssistantMessage.emit(res.assistantMessageId);
            this.updateUserMessage.emit(res.userMessageId);
            this.shouldScroll = true;
            this.messages.push({
              id: res.assistantMessageId,
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
            this.factsChanged.emit(this.facts);

            this.loading = false;
          },
          error: (err) => {
            console.error('Error in explain mode:', err);
            this.errorRetryButton = true;
            this.loading = false;
          },
        });
    } else {
      if (window.sessionStorage.getItem('needsAuthentication') === 'true') {
        this.oidcSecurityService
          .getAccessToken()
          .subscribe((accessToken: string | undefined) => {
            this.sendMessageAndResponseStream(question, accessToken);
          });
      } else {
        this.sendMessageAndResponseStream(question, undefined);
      }
    }
  }

  async retryMessageInNewChat(message: Message) {
    this.messages = [];
    await this.executeChatCreation(); // Waits for the chat creation to complete
    this.sendMessage(message.text);
    this.errorRetryButton = false;
  }

  sendMessageAndResponseStream(
    question: string,
    accessToken: string | undefined
  ) {
    this.streamService
      .getStreamDataNew(
        question,
        this.activeChat.id,
        this.activeChat.sibyllaId,
        accessToken
      )
      .subscribe({
        next: (streamMessage: {
          AssistantMessageId: any;
          Delta: { Content: string };
        }) => {
          //console.log(r);
          if (this.loading) {
            this.messages.push({
              id: streamMessage.AssistantMessageId,
              text: '' + streamMessage.Delta?.Content,
              sender: 'assistant',
              timestamp: new Date().toLocaleTimeString().slice(0, 5),
              completed: false,
            });
            this.loading = false;
            this.clearSession = false;
          } else {
            this.messages[this.messages.length - 1].text =
              this.messages[this.messages.length - 1].text +
              streamMessage.Delta?.Content;
          }
          this.shouldScroll = true;
        },
        error: (err: any) => {
          console.error('Error receiving stream data:', err);
          this.loading = false;
          this.shouldScroll = false;
          this.errorRetryButton = true;
        },
        complete: () => {
          // after 2 seconds stop shouldScroll
          setTimeout(() => {
            this.shouldScroll = false;
          }, 2000);
          this.loading = false;
          // if text parameter is defined, read the last message text with the WEB SPEECH API
          this.setAllMessagesCompleted();
        },
      });
  }
  ngAfterViewChecked() {
    if (this.shouldScroll) {
      this.scrollToBottom();
      this.shouldScroll = false;
    }
  }

  private scrollToBottom(): void {
    try {
      this.messagesContainer.nativeElement.scrollTo({
        top: this.messagesContainer.nativeElement.scrollHeight + 100,
        behavior: 'smooth',
      });
    } catch (err) {}
  }

  //keypress event enter
  onKey(event: any) {
    if (
      event.key === 'Enter' &&
      !this.loading &&
      this.chatInput !== '' &&
      !this.errorRetryButton
    ) {
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
    if (!newFeedback) {
      this.openDialogFeedback(messageId, newFeedback);
    } else {
      // give feedback to the server without opening the dialog
      const feedbackData: {
        chatId: string;
        messageId: string;
        rating: string;
      } = {
        chatId: this.activeChat.id,
        messageId: messageId,
        rating: 'positive',
      };
      this.authService.giveFeedback(feedbackData).subscribe({
        next: (response: any) =>
          console.log('Feedback submitted successfully', response),
        error: (error: any) =>
          console.error('Error submitting feedback', error),
      });
    }
  }

  openDialogFeedback(messageId: string, initialFeedback: boolean) {
    const dialogRef = this.dialogFeedback.open(DialogFeedbackComponent, {
      minWidth: '50%',
    });

    dialogRef.afterClosed().subscribe((result) => {
      // Check if the dialog was not closed with a "cancel" result
      if (result !== 'cancel') {
        const feedbackText = result as string | undefined;
        const messageIndex = this.messages.findIndex((m) => m.id === messageId);
        if (messageIndex !== -1) {
          if (feedbackText !== undefined) {
            this.messages[messageIndex].feedbackText = feedbackText;
          }

          // Initialize feedbackData with all potential properties
          const feedbackData: {
            chatId: string;
            messageId: string;
            text?: string; // Mark as optional
            rating: string;
          } = {
            chatId: this.activeChat.id,
            messageId: messageId,
            rating: initialFeedback ? 'positive' : 'negative',
          };

          // Conditionally add the text property only if it's not undefined
          if (feedbackText !== undefined) {
            feedbackData.text = feedbackText;
          }

          // Send feedback to server
          this.authService.giveFeedback(feedbackData).subscribe({
            next: (response: any) =>
              console.log('Feedback submitted successfully', response),
            error: (error: any) =>
              console.error('Error submitting feedback', error),
          });
        }
      } else {
        // reset feedback to undefined
        const messageIndex = this.messages.findIndex((m) => m.id === messageId);
        if (messageIndex !== -1) {
          this.messages[messageIndex].feedback = undefined;
          this.messages[messageIndex].feedbackText = undefined;
        }
      }
    });
  }

  copyContent(message: Message): void {
    let textToCopy = message.text;

    navigator.clipboard.writeText(textToCopy).then(
      () => {
        console.log('Text copied successfully', textToCopy);
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
  convertTextToLinks(text: string) {
    if (!text) return text;
    // Adjusted regex to detect URLs within parentheses.
    const urlRegex = /\(((https?:\/\/)|(www\.))[^\s]+\)/g;
    return this.sanitizer.bypassSecurityTrustHtml(
      text.replace(urlRegex, (urlMatch) => {
        let url = urlMatch.slice(1, -1); // Remove the parentheses
        // Ensure the URL starts with http/https.
        let hyperlink = url.startsWith('http') ? url : 'http://' + url;

        const link = document.createElement('a');
        link.href = encodeURI(hyperlink);
        link.textContent = hyperlink;
        link.target = '_blank';
        link.classList.add('custom-link');

        return link.outerHTML;
      })
    );
  }
}
