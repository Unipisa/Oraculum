import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { Subscription } from 'rxjs';
import { SpeechRecognitionService } from 'src/app/services/speech-recognition.service';

@Component({
  selector: 'app-speech-recognition-modal',
  templateUrl: './speech-recognition-modal.component.html',
  styleUrls: ['./speech-recognition-modal.component.scss'],
})
export class SpeechRecognitionModalComponent implements OnInit, OnDestroy {
  speechData: string = '';
  private speechSubscription!: Subscription;

  constructor(
    public dialogRef: MatDialogRef<SpeechRecognitionModalComponent>,
    private speechRecognitionService: SpeechRecognitionService
  ) {}

  ngOnInit() {
    this.speechSubscription = this.speechRecognitionService.record().subscribe({
      next: (value) => {
        this.speechData = value;
      },
      error: (err) => {
        console.log(err);
        if (err.error === 'no-speech') {
          console.log('No speech was detected. Try again.');
        }
      },
      complete: () => {
        console.log('Speech recognition service is done');
        this.dialogRef.close(this.speechData);
      },
    });
  }

  ngOnDestroy() {
    this.speechSubscription.unsubscribe();
    this.speechRecognitionService.stop();
  }

  onClose(): void {
    this.dialogRef.close();
  }
}
