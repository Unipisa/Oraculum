import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ChatComponent } from './components/chat/chat.component';
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
import { NgxSkeletonLoaderModule } from 'ngx-skeleton-loader';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthConfigModule } from './auth/auth-config.module';
import { AuthInterceptor } from 'angular-auth-oidc-client';
import { SpeechRecognitionModalComponent } from './components/speech-recognition-modal/speech-recognition-modal.component';

@NgModule({
  declarations: [
    AppComponent,
    ChatComponent,
    ChatExplainComponent,
    LayoutComponent,
    KnowledgeComponent,
    KpiComponent,
    ConfigComponent,
    DialogFeedbackComponent,
    DialogConfirmComponent,
    LoginComponent,
    ImportDialog,
    FactDialogComponent,
    ErrorDialogComponent,
    ImportDialogComponent,
    SelectionComponent,
    ProfileComponent,
    SpeechRecognitionModalComponent,
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
    NgxSkeletonLoaderModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    AuthConfigModule,
  ],
  providers: [
    PredictService,
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
  ],
  bootstrap: [AppComponent],
  exports: [FormsModule],
})
export class AppModule {}
