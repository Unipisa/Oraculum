import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { KnowledgeService, taskInfo } from 'src/app/services/knowledge.service';
import language from 'src/app/config/language';
import { SnackbarService } from 'src/app/services/snackbar.service';

@Component({
  selector: 'app-import-dialog',
  templateUrl: './import-dialog.component.html',
  styleUrls: ['./import-dialog.component.scss'],
})
export class ImportDialogComponent {
  language = language;
  importForm!: FormGroup;
  fileName!: string;
  loading = false;
  category!: string;

  constructor(
    private fb: FormBuilder,
    private dialog: MatDialogRef<ImportDialogComponent>,
    private knowledgeService: KnowledgeService,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private sbService: SnackbarService
  ) {
    this.createForm();
    this.category = data.category;
  }

  createForm() {
    this.importForm = this.fb.group({
      label: ['', Validators.required],
      url: ['', [Validators.required, Validators.pattern(/http(s)?:\/\/.+/)]], // Validatore di URL
      // type: ['', Validators.required],
      // expiration: [null],
      file: [null], // Questo sarà gestito separatamente dal metodo onFileSelected
    });
  }

  isDocumentSelected() {
    return this.importForm.get('label')?.value === 'file';
  }

  isUrlSelected() {
    return this.importForm.get('label')?.value === 'url';
  }

  get url() {
    return this.importForm.get('url');
  }

  onSubmit() {
    if (this.importForm.valid || this.fileName) {
      console.log(this.importForm.value);
      this.loading = true;
      if (this.isDocumentSelected() && this.importForm.get('file')?.value) {
        const file = this.importForm.get('file')?.value;
        const fileType = file.type;
        if (
          fileType === 'application/pdf' ||
          fileType ===
            'application/vnd.openxmlformats-officedocument.wordprocessingml.document'
        ) {
          this.knowledgeService.ingestDocument(file, this.category).subscribe({
            next: (task: taskInfo) => {
              console.log(task);
              this.loading = false;
              this.checkTaskStatus(task.taskId);
            },
            error: (err) => {
              console.error('Error:', err);
            },
          });
        } else if (
          fileType.startsWith('video/') ||
          fileType.startsWith('audio/')
        ) {
          this.knowledgeService
            .ingestAudioVideo(file, this.category)
            .subscribe({
              next: (task: taskInfo) => {
                console.log(task);
                this.loading = false;
                this.checkTaskStatus(task.taskId);
              },
              error: (err) => {
                console.error('Error:', err);
              },
            });
        } else {
          console.log('Unsupported file type');
          this.loading = false;
        }
      } else if (this.isUrlSelected()) {
        this.knowledgeService
          .ingestWebpage([this.url?.value], this.category)
          .subscribe({
            next: (task: taskInfo) => {
              console.log(task);
              this.loading = false;
              this.checkTaskStatus(task.taskId);
            },
            error: (err) => {
              console.error('Error:', err);
            },
          });
      }
    } else {
      console.log('Check form ', this.importForm.value);
      this.loading = false;
      this.importForm.markAllAsTouched();
    }
  }

  private checkTaskStatus(taskId: string): void {
    this.knowledgeService.checkTaskStatus(taskId).subscribe({
      next: (task: taskInfo) => {
        if (task.status !== 'Error') {
          this.sbService.showSnackbar(
            this.language.requestSuccess,
            'Chiudi',
            2000
          );
        } else {
          this.sbService.showSnackbar(this.language.requestErr, 'Chiudi', 2000);
        }
      },
      error: (err) => {
        console.error('Error:', err);
      },
    });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.fileName = file.name;
      this.importForm.patchValue({ file: file });
      console.log('File selected and form updated with:', file);
      setTimeout(
        () =>
          console.log(
            'Form value after file selection:',
            this.importForm.value
          ),
        0
      );
    }
  }
}
