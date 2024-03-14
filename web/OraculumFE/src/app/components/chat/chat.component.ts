import { BotInfo } from './../../interfaces/botInfo';
import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { Message } from 'src/app/interfaces/message';
import { PredictService } from 'src/app/services/predict.service';
import { StreamService } from 'src/app/services/stream.service';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { v4 as uuidv4 } from 'uuid';
import { MatDialog } from '@angular/material/dialog';
import { DialogFeedbackComponent } from '../dialogs/dialog-feedback/dialog-feedback.component';
import { AuthService } from 'src/app/services/auth.service';
import { Chat } from 'src/app/interfaces/chat';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Subscription } from 'rxjs';
import { SpeechRecognitionModalComponent } from '../speech-recognition-modal/speech-recognition-modal.component';
import language from 'src/app/config/language';

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss'],
})
export class ChatComponent implements OnInit {
  language = language;
  botInfo: BotInfo = this.authService.selectedBot;
  activeChat!: Chat;
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
  loadingChats: boolean = false;

  // array of random messages to select if the bot doesn't know the answer
  @ViewChild('messagesContainer') private messagesContainer!: ElementRef;
  chatInput: string = '';
  loading: boolean = false;
  loadingText: string = '';
  buttonDisabled = false;
  clearSession: boolean = true;
  text = '';

  constructor(
    private predictService: PredictService,
    private streamService: StreamService,
    private sanitizer: DomSanitizer,
    public dialogFeedback: MatDialog,
    private authService: AuthService,
    private oidcSecurityService: OidcSecurityService
  ) {}

  ngOnInit() {
    this.newChat();
  }

  newChat() {
    if (this.buttonDisabled) return;
    this.loading = true;
    this.buttonDisabled = true;
    this.authService.newChat('normal').subscribe({
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
      error: (err: any) => {
        console.error('Error:', err);
      },
      complete: () => {
        this.loading = false; // Stop loading once the request completes
        setTimeout(() => (this.buttonDisabled = false), 5000); // Re-enable the button after 5 seconds
      },
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
        //console.log('The dialog was closed with result:', result);
        // Process the result here
        //console.log('Result:', result);
        this.sendMessage(result);
      }
    });
  }

  linkify(text: string) {
    const urlRegex =
      /(\b(https?|ftp|file):\/\/[-A-Z0-9+&@#\/%?=~_|!:,.;]*[-A-Z0-9+&@#\/%=~_|])/gi;
    const replacedText = text.replace(urlRegex, (url) => {

      const link = document.createElement("a");
      link.href = encodeURI(url);
      link.textContent = url;
      link.target = "_blank";
      link.classList.add("custom-link");

      return link.outerHTML;
    });
    return replacedText;
  }

  sendMessage(text?: string) {
    if (
      (this.chatInput != '' &&
        this.chatInput != null &&
        this.chatInput != undefined) ||
      (text != '' && text != null && text != undefined)
    ) {
      this.messages.push({
        id: uuidv4(),
        text: !!text ? text : this.chatInput,
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
      //console.log('Question:', question);

      if(window.sessionStorage.getItem('needsAuthentication') === 'true'){
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

  sendMessageAndResponseStream(question: string, accessToken: string | undefined){
    //console.log('Access Token:', accessToken);
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
      },
      error: (err: any) => {
        console.error('Error receiving stream data:', err);
        this.loading = false;
      },
      complete: () => {
        //console.log('Stream data completed');
        this.loading = false;
        // if text parameter is defined, read the last message text with the WEB SPEECH API
        this.setAllMessagesCompleted();
      },
    });
  }
  // readMessage(text: string) {
  //   if ('speechSynthesis' in window) {
  //     console.log('Speech synthesis supported');
  //     // Speech synthesis supported
  //     var msg = new SpeechSynthesisUtterance();
  //     msg.text = text;
  //     msg.lang = 'it-IT'; // Language to use
  //     msg.pitch = 1; // Pitch, can range from 0 to 2
  //     msg.rate = 1; // Rate, can be 0.1 to 10

  //     window.speechSynthesis.cancel();
  //     window.speechSynthesis.speak(msg);
  //   } else {
  //     // Speech synthesis not supported
  //     console.error('Speech synthesis not supported');
  //   }
  // }

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

  closeChat() {
    //console.log('close chat');
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
    this.openDialogFeedback(messageId, newFeedback);
  }

  openDialogFeedback(messageId: string, initialFeedback: boolean) {
    const dialogRef = this.dialogFeedback.open(DialogFeedbackComponent, {
      minWidth: '50%',
    });

    dialogRef.afterClosed().subscribe((feedbackText?: string) => {
      /*
      console.log(
        'The dialog was closed',
        feedbackText !== undefined
          ? `with text: ${feedbackText}`
          : 'without text'
      );
      */
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
    });
  }

  copyContent(content: string): void {
    navigator.clipboard.writeText(content).then(
      () => {
        console.log('Text copied successfully', content);
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
    // console.log(this.messages);
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

        const link = document.createElement("a");
        link.href = encodeURI(hyperlink);
        link.textContent = hyperlink;
        link.target = "_blank";
        link.classList.add("custom-link");

        return link.outerHTML;
      })
    );
  }
}
