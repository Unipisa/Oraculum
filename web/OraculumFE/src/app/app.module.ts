import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ChatComponent } from './components/chat/chat.component';
import { FactCardComponent } from './components/fact-card/fact-card.component';
import { FactListComponent } from './components/fact-list/fact-list.component';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PredictService } from './services/predict.service';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { LayoutComponent } from './components/layout/layout.component';
import { KnowledgeComponent } from './components/knowledge/knowledge.component';
import { KpiComponent } from './components/kpi/kpi.component';
import { ConfigComponent } from './components/config/config.component';
import { MatTabsModule } from '@angular/material/tabs';
import { ChatExplainComponent } from './components/chatExplain/chatExplain.component';
import { MatMenuModule } from '@angular/material/menu';
import { MatNativeDateModule, MatRippleModule } from '@angular/material/core';
import { DialogFeedbackComponent } from './components/dialogs/dialog-feedback/dialog-feedback.component';
import { DialogConfirmComponent } from './components/dialogs/dialog-confirm/dialog-confirm.component';
import { MatDialogModule } from '@angular/material/dialog';
import { LoginComponent } from './components/login/login.component';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';
import { ImportDialog } from './components/kpi/ImportDialog/import-dialog.component';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { FactDialogComponent } from './components/dialogs/fact-dialog/fact-dialog.component';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatChipsModule } from '@angular/material/chips';
import { ErrorDialogComponent } from './components/dialogs/dialog-error';
import { ImportDialogComponent } from './components/dialogs/import-dialog/import-dialog.component';
import { MatSelectModule } from '@angular/material/select';
import { SelectionComponent } from './components/selection/selection.component';
import { MatCardModule } from '@angular/material/card';
import { ProfileComponent } from './components/profile/profile.component';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthConfigModule } from './auth/auth-config.module';
import { AuthInterceptor } from 'angular-auth-oidc-client';
import { SpeechRecognitionModalComponent } from './components/speech-recognition-modal/speech-recognition-modal.component';
import { MarkdownModule } from 'ngx-markdown';
import { AddSourceDialogComponent } from './components/dialogs/add-source-dialog/add-source-dialog.component';
import { HeaderComponent } from './components/header/header.component';
import { KnowledgeSearchComponent } from './components/knowledge-search/knowledge-search.component';

import { ChatModularComponent } from './components/chat-modular/chat-modular.component';
import {
  provideDesignAngularKit,
  DesignAngularKitModule,
  DesignAngularKitConfig,
} from 'design-angular-kit';
import { ConfirmDialogComponent } from './components/dialogs/confirm-newchat/confirm-newChat.component';
import { DeviceDetectorService } from 'ngx-device-detector';

// Puoi aggiungere alla libreria una configurazione iniziale
const initConfig: DesignAngularKitConfig | undefined = {
  /**
   * The bootstrap-italia asset folder path
   * @default ./bootstrap-italia
   */
  assetBasePath: './bootstrap-italia',

  /**
   * Load the <a href="https://italia.github.io/bootstrap-italia/docs/come-iniziare/introduzione/#fonts">bootstrap-italia fonts</a>
   * @default true
   */
  loadFont: true,
};

provideDesignAngularKit(initConfig);

DesignAngularKitModule.forRoot(initConfig);

@NgModule({
  declarations: [
    ConfirmDialogComponent,
    AppComponent,
    ChatComponent,
    FactCardComponent,
    FactListComponent,
    ChatExplainComponent,
    LayoutComponent,
    KnowledgeComponent,
    KpiComponent,
    ConfigComponent,
    DialogFeedbackComponent,
    DialogConfirmComponent,
    LoginComponent,
    ImportDialog,
    AddSourceDialogComponent,
    FactDialogComponent,
    ErrorDialogComponent,
    ImportDialogComponent,
    SelectionComponent,
    ProfileComponent,
    SpeechRecognitionModalComponent,
    ChatModularComponent,
    KnowledgeSearchComponent,
  ],
  imports: [
    BrowserModule,
    MatPaginatorModule,
    MatTableModule,
    AppRoutingModule,
    MatButtonModule,
    MatIconModule,
    FormsModule,
    HttpClientModule,
    MatTabsModule,
    BrowserAnimationsModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatMenuModule,
    MatRippleModule,
    MatDialogModule,
    MatSnackBarModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatChipsModule,
    MatSelectModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    AuthConfigModule,
    HeaderComponent,
    MarkdownModule.forRoot(),
    DesignAngularKitModule.forRoot(),
  ],
  providers: [
    PredictService,
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
  ],
  bootstrap: [AppComponent],
  exports: [FormsModule],
})
export class AppModule {}
