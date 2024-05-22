import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import language from 'src/app/config/language';

@Component({
  selector: 'app-confirm-newChat-dialog',
  templateUrl: './confirm-newChat-dialog.component.html',
  styleUrls: ['./confirm-newChat-dialog.component.css']
})
export class ConfirmDialogComponent {
    language = language;
    
  constructor(
    public dialogRef: MatDialogRef<ConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { message: string }
  ) {}

  onConfirm(): void {
    this.dialogRef.close(true);
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}