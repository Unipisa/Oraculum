import { Component, Inject, OnInit } from '@angular/core';
import {
  MatDialog,
  MAT_DIALOG_DATA,
  MatDialogRef,
} from '@angular/material/dialog';
import language from 'src/app/config/language';

@Component({
  selector: 'app-dialog-feedback',
  templateUrl: './dialog-feedback.component.html',
  styleUrls: ['./dialog-feedback.component.scss'],
})
export class DialogFeedbackComponent implements OnInit {
  language = language;

  feedbackText = '';

  constructor(
    public dialogRef: MatDialogRef<DialogFeedbackComponent>
  ) // @Inject(MAT_DIALOG_DATA) public data
  {}

  onNoClick(): void {
    this.dialogRef.close('cancel');
  }

  ngOnInit() {}
}
