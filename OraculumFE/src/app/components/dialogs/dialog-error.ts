// error-dialog.component.ts
import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-error-dialog',
  template: `
  <div class="error-component">
    <h1 mat-dialog-title>
      Errore
    </h1>
    <div mat-dialog-content class="custom-dialog-content">
      <p>{{ errorMessage }}</p>
    </div>
    </div>
    <div mat-dialog-actions class="custom-dialog-actions">
      <button mat-flat-button (click)="closeDialog()">Chiudi</button>
    </div>
  `,
  styles: [
    `
      .error-icon {
        margin-right: 8px;
        vertical-align: middle;
      }

      .custom-dialog-content {
        // padding: 16px;
      }

      .custom-dialog-actions {
        display: flex;
        justify-content: flex-end;
        align-items: flex-end;
        // padding: 16px;
      }
      .error-component{
        // padding: 40px;
      }
    `,
  ],
})
export class ErrorDialogComponent {
  constructor(
    @Inject(MAT_DIALOG_DATA) public errorMessage: string,
    private dialogRef: MatDialogRef<ErrorDialogComponent>
  ) {}

  closeDialog(): void {
    this.dialogRef.close();
  }
}
