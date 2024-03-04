import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-import-dialog',
  templateUrl: './import-dialog.component.html',
  styleUrls: ['./import-dialog.component.scss'],
})
export class ImportDialogComponent {
  importForm!: FormGroup;
  fileName!: string;

  constructor(
    private fb: FormBuilder,
    private dialog: MatDialogRef<ImportDialogComponent>
  ) {
    this.createForm();
  }

  createForm() {
    this.importForm = this.fb.group({
      label: ['', Validators.required],
      url: ['', [Validators.required, Validators.pattern(/http(s)?:\/\/.+/)]], // Validatore di URL
      type: ['', Validators.required],
      expiration: [null],
      file: [null], // Questo sarà gestito separatamente dal metodo onFileSelected
    });
  }

  isDocumentSelected() {
    return this.importForm.get('label')?.value === 'document';
  }

  isUrlSelected() {
    return this.importForm.get('label')?.value === 'url';
  }

  get url() {
    return this.importForm.get('url');
  }

  onSubmit() {
    if (this.importForm.valid) {
      console.log(this.importForm.value);
      // Gestire la sottomissione del form qui
      this.dialog.close(this.importForm.value);
    } else {
      this.importForm.markAllAsTouched();
    }
  }
  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.fileName = file.name;
    }
  }
}
