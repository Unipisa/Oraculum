import { Component, OnInit } from '@angular/core';
import * as FileSaver from 'file-saver';
import { GlobalVariableService } from '../service';
import { ErrorDialogComponent} from '../../dialogs/dialog-error';
import { MatDialog } from '@angular/material/dialog';
import language from 'src/app/config/language';

interface QuestionData {
  question: string;
  ground_truths: string[];
}

@Component({
  selector: 'app-import-dialog',
  templateUrl: './import-dialog.component.html',
  styleUrls: ['./import-dialog.component.scss']
})
export class ImportDialog implements OnInit {

  language = language;

  public myset: any;

  constructor(private globalVariableService: GlobalVariableService,private dialog: MatDialog) {
    this.globalVariableService.sharedVariable = null
  }

  ngOnInit() {

  }

  file: File | null = null;
  fileContent: string | null = null;
  private jsonData: QuestionData[] = [];

  onFileDropped(event: DragEvent): void {
    event.preventDefault();
    const droppedFiles = event.dataTransfer?.files;

    if (droppedFiles && droppedFiles.length > 0) {
      this.readFile(droppedFiles[0]);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  private readFile(file: File): void {
    const reader = new FileReader();

    reader.onload = () => {
      this.file = file;
      this.fileContent = reader.result as string;
      this.printFileContentToConsole();
      this.parseCsvToJson();
    };

    reader.readAsText(file);
  }

  private printFileContentToConsole(): void {
    console.log('Contenuto del file:', this.fileContent);
  }

  private parseCsvToJson(): void {
    const lines = this.fileContent?.split('\n');

    if (lines) {
      lines.forEach(line => {
        const [question, answer] = line.split(',');

        if (question && answer) {
          const jsonEntry = {
            question: question.trim(),
            ground_truths: [answer.trim()]
          };

          this.jsonData.push(jsonEntry);
        }
      });

      console.log('Contenuto trasformato in JSON:', this.jsonData);
      this.globalVariableService.sharedVariable = this.jsonData
    } else {
      console.error('Nessuna linea trovata nel contenuto del file.');
    }
  }

  public saveJsonToFile(): void {
    if (this.globalVariableService.sharedVariable) {
      const jsonData = JSON.stringify(this.globalVariableService.sharedVariable);
      const blob = new Blob([jsonData], { type: 'application/json' });
      const fileName = 'data.json';

      FileSaver.saveAs(blob, fileName);
    } else {
      this.showErrorDialog("Set non caricato");
    }
  }


  showErrorDialog(errorMessage: string): void {
    this.dialog.open(ErrorDialogComponent, {
      data: errorMessage,
      minWidth: '30%'
    });
  }

}
