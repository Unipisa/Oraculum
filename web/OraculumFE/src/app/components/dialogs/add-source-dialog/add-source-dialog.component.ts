import { Component } from '@angular/core';
import { ItUploadDragDropComponent } from 'design-angular-kit';
import language from 'src/app/config/language';
import { ViewChild } from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  UntypedFormGroup,
  Validators,
} from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SnackbarService } from 'src/app/services/snackbar.service';
import * as validator from 'validator'; // Import validator.js

@Component({
  selector: 'app-add-source-dialog',
  templateUrl: './add-source-dialog.component.html',
  styleUrls: ['./add-source-dialog.component.scss'],
})
export class AddSourceDialogComponent {
  isUploading = false;
  isError = false;
  isCompleted = false;
  data: File | undefined;
  type: string = '';
  language = language;
  tabs = [language.loadfile, language.loadlink, language.loadText];
  selectedIndex = 0;
  selectAfterAdding = false;
  @ViewChild('uploadDragDropComponent')
  uploadDragDropComponent!: ItUploadDragDropComponent;
  myForm: FormGroup;
  urlForm: FormGroup;

  constructor(
    private _fb: FormBuilder,
    public dialogRef: MatDialogRef<AddSourceDialogComponent>,
    private snackBar: MatSnackBar,
    private snackbarService: SnackbarService
  ) {
    const validatorsText = [
      Validators.required,
      Validators.maxLength(1200),
      Validators.minLength(50),
    ];

    this.myForm = this._fb.group({
      textarea: ['', validatorsText],
    });

    this.urlForm = this._fb.group({
      url: ['', [Validators.required, this.urlValidator]],
    });
  }

  urlValidator(control: FormControl): { [key: string]: any } | null {
    if (!control.value) {
      return null; // Don't validate empty values to allow optional controls
    }
    return validator.isURL(control.value, {
      protocols: ['http', 'https'],
      require_protocol: true,
    })
      ? null
      : { invalidUrl: true };
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(type: 'file' | 'link' | 'content'): void {
    const url = this.urlForm.get('url')?.value;
    const textarea = this.myForm.get('textarea')?.value;

    if (type == 'file' && this.data) {
      const resultData = {
        message: this.data?.name,
        content: this.data,
        type: this.type,
      };
      this.dialogRef.close(resultData);
      this.snackbarService.showSnackbar(
        this.data?.name + ' ' + language.Uploadsuccessful,
        'Chiudi',
        5000
      );
    } else if (type == 'link' && url) {
      this.snackbarService.showSnackbar(
        'URL: ' + url + ' ' + language.Uploadsuccessful,
        'Chiudi',
        5000
      );

      const resultData = {
        message: 'url',
        content: url,
        type: 'url',
      };
      this.dialogRef.close(resultData);
    } else if (type == 'content' && textarea) {
      this.snackbarService.showSnackbar(
        'Testo ' + language.Uploadsuccessful,
        'Chiudi',
        5000
      );
      const resultData = {
        message: 'text',
        content: textarea,
        type: 'text',
      };
      this.dialogRef.close(resultData);
    } else {
      this.snackbarService.showSnackbar('Errore', 'Chiudi', 2000);
    }
  }

  value = '';
  savedValue = undefined;

  save(form: UntypedFormGroup) {
    this.savedValue = form.value.myInput;
  }

  markAllAsTouched() {
    this.myForm.markAllAsTouched();
  }
  onDragUploadStart(file: File): void {
    // Lista dei formati di file accettati
    const acceptedFormats = [
      'audio/mp3',
      'video/mp4',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
      'application/msword',
      'application/pdf',
    ];
    this.isUploading = true;

    if (!acceptedFormats.includes(file.type)) {
      this.snackBar.open(language.unsupportedformat, 'Close', {
        duration: 5000,
        panelClass: ['snackbar-error'],
      });
      return;
    }

    const reader = new FileReader();

    reader.onload = () => {
      this.uploadDragDropComponent.progress(100);
    };

    reader.onerror = () => {
      this.snackBar.open(language.Errorreadingfile, 'Close', {
        duration: 5000,
        panelClass: ['snackbar-error'],
      });
    };
    if (file.type === 'application/pdf' || file.type.includes('word')) {
      reader.readAsArrayBuffer(file);
      this.data = file;
      this.type = file.type;
      this.isCompleted = true;
    } else {
      reader.readAsDataURL(file);
      this.data = file;
      this.type = file.type;
      this.isCompleted = true;
    }
  }
}
