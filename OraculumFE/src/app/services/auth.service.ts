import { Injectable } from '@angular/core';
import {
  Observable,
  catchError,
  throwError,
  BehaviorSubject,
  tap,
  map,
} from 'rxjs';
import { Router } from '@angular/router';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { BotInfo } from '../interfaces/botInfo';
import { Chat } from '../interfaces/chat';
import {
  FeedbackOptions,
  FeedbackBase,
  FeedbackWithMessageId,
  FeedbackWithFactId,
} from '../interfaces/feedback';
import { OidcSecurityService } from 'angular-auth-oidc-client';
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  isAuthenticated = this.isAuthenticatedSubject.asObservable();
  currentBot: BotInfo = {
    id: 'BotName',
    baseAssistantPrompt: 'BotPrompt',
  };
  availableBots: any;
  private visibleBotsCount = new BehaviorSubject<number>(0);
  private titleSource = new BehaviorSubject<string>('Sibylla Backoffice');
  currentTitle = this.titleSource.asObservable();
  lastChats: { [botId: string]: { chatId?: string; chatEid?: string } } = {};
  private currentUserSubject = new BehaviorSubject<any>(null);
  currentUser = this.currentUserSubject.asObservable();

  constructor(
    private router: Router,
    private http: HttpClient,
    private oidcSecurityService: OidcSecurityService
  ) {
    this.initializeLastChats();
  }

  private initializeLastChats() {
    const lastChatsStored = localStorage.getItem('lastChats');
    this.lastChats = lastChatsStored ? JSON.parse(lastChatsStored) : {};
  }

  private updateLastChats(botId: string, chatId?: string, chatEid?: string) {
    if (chatId !== undefined) {
      this.lastChats[botId] = { ...this.lastChats[botId], chatId };
    }
    if (chatEid !== undefined) {
      this.lastChats[botId] = { ...this.lastChats[botId], chatEid };
    }
    localStorage.setItem('lastChats', JSON.stringify(this.lastChats));
  }

  login(): Observable<any> {
    return this.http.get<any>('/api/oraculum/v1/User/me').pipe(
      tap((data) => {
        console.log(data);
        this.currentUserSubject.next(data);
        this.isAuthenticatedSubject.next(true); // Update authentication state
        this.router.navigate(['/selection']); // Navigate on success
      }),
      catchError((err: HttpErrorResponse) => {
        if (err.status === 401) {
          console.log(err);
          window.sessionStorage.setItem('needsAuthentication', 'true')
          this.oidcSecurityService.authorize();
        } else {
          // go to login page on error
          this.router.navigate(['/login']);
        }
        throw err; // Rethrow the error if you still want to handle it elsewhere
      })
    );
  }

  logout() {
    this.isAuthenticatedSubject.next(false);
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }

  // get isLoggedIn(): boolean {
  //   return this.isAuthenticated;
  // }

  get selectedBot(): BotInfo {
    return this.currentBot;
  }

  getMyBots(): Observable<BotInfo[]> {
    // Explicitly declare the return type of the HTTP request as BotInfo[]
    return this.http
      .get<BotInfo[]>(`/api/oraculum/v1/FrontOffice/sibylla`)
      .pipe(
        catchError(this.handleError.bind(this)),
        map((bots: BotInfo[]) => {
          const visibleBots = bots.filter((bot) => !bot.hidden);
          this.visibleBotsCount.next(visibleBots.length);
          return visibleBots;
        })
      );
  }

  getVisibleBotsCount(): Observable<number> {
    return this.visibleBotsCount.asObservable();
  }

  selectBot(b: BotInfo) {
    this.currentBot = b;
    return this.router.navigate(['/chat']);
  }

  getMyChats(): Observable<any> {
    const sibyllaId = this.selectedBot.id;
    return this.http.get(
      `/api/oraculum/v1/FrontOffice/sibylla/${sibyllaId}/chat`
    );
  }

  newChat(type: 'normal' | 'explain'): Observable<Chat> {
    const sibyllaId = this.selectedBot.id;
    return this.http
      .post(`/api/oraculum/v1/FrontOffice/sibylla/${sibyllaId}/chat`, {
        type: type,
      })
      .pipe(
        tap((response: any) => {
          console.log(response);

          // Based on the type, decide whether to update chatId or chatEid.
          if (type === 'normal') {
            // If the chat type is normal, update chatId with the response's chatId.
            this.updateLastChats(sibyllaId, response.id);
          } else if (type === 'explain') {
            this.updateLastChats(sibyllaId, undefined, response.id);
          }
        }),
        catchError(this.handleError.bind(this))
      );
  }

  getMessages(sibyllaId: string, chatId: string): Observable<any[]> {
    return this.http
      .get<any[]>(
        `/api/oraculum/v1/FrontOffice/sibylla/${sibyllaId}/chat/${chatId}`
      )
      .pipe(
        catchError((error: HttpErrorResponse) => {
          console.error('An error occurred while fetching messages:', error);
          return throwError(() => new Error('Error fetching messages'));
        })
      );
  }

  sendMessage(text: string, chatId: string): Observable<any> {
    const sibyllaId = this.selectedBot.id;
    return this.http.post(
      `/api/oraculum/v1/FrontOffice/sibylla/${sibyllaId}/chat/${chatId}/message`,
      { text: text }
    );
  }

  giveFeedback(options: FeedbackOptions): Observable<any> {
    if (options.chatId && options.factId) {
      throw new Error('chatId and factId are mutually exclusive');
    }

    const sibyllaId = this.selectedBot.id;
    let endpoint = '';
    let body: FeedbackBase | FeedbackWithMessageId | FeedbackWithFactId = {
      rating: options.rating,
    };

    if (options.text) {
      body.text = options.text;
    }

    if ('messageId' in options && options.chatId) {
      endpoint = `/api/oraculum/v1/FrontOffice/sibylla/${sibyllaId}/chat/${options.chatId}/feedback`;
      body = { ...body, messageId: options.messageId };
    } else if ('factId' in options) {
      endpoint = `/api/oraculum/v1/FrontOffice/sibylla/${sibyllaId}/fact/${options.factId}/feedback`;
      body = { ...body, factId: options.factId };
    } else {
      throw new Error(
        'Either chatId with messageId or factId must be provided'
      );
    }

    return this.http.post(endpoint, body);
  }

  private handleError(error: HttpErrorResponse) {
    if (error.error instanceof ErrorEvent) {
      console.error('An error occured:', error.error.message);
    } else {
      console.error(
        `Backend returned code "${error.status}", body was "${error.statusText}"`
      );
    }

    return throwError(error);
  }
}
