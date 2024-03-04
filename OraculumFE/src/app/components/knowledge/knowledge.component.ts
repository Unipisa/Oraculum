import { Component, OnInit } from '@angular/core';
import { KnowledgeService } from 'src/app/services/knowledge.service';
import { Validators, FormBuilder } from '@angular/forms';
import { Fact } from 'src/app/interfaces/fact';
import { SnackbarService } from 'src/app/services/snackbar.service';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { Observable, map } from 'rxjs';
import {
  ConfirmationDialogData,
  DialogConfirmComponent,
} from '../dialogs/dialog-confirm/dialog-confirm.component';
import { FactDialogComponent } from '../dialogs/fact-dialog/fact-dialog.component';
import { ImportDialogComponent } from '../dialogs/import-dialog/import-dialog.component';
import language from 'src/app/config/language';

interface SearchPayload {
  query: string;
  distance: number;
  limit: number;
  factTypeFilter?: string[];
  categoryFilter?: string[];
}

@Component({
  selector: 'app-knowledge',
  templateUrl: './knowledge.component.html',
  styleUrls: ['./knowledge.component.scss'],
})
export class KnowledgeComponent implements OnInit {
  language = language;
  
  searchForm = this.fb.group({
    searchInput: ['', Validators.required],
    tipoInput: [''],
  });

  results!: Fact[];

  selectedResult: Fact | null = null;

  categoryFilter: string[] = [];

  constructor(
    private knowledgeService: KnowledgeService,
    private fb: FormBuilder,
    private snackbarService: SnackbarService,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.knowledgeService.getSibyllaeConfigs().subscribe((response) => {
      const selectedSibylla = localStorage.getItem('selectedSibylla');
      var categoryFilter = response.find((c) => c.id == selectedSibylla)
        ?.memoryConfiguration.categoryFilter![0];
      if (categoryFilter) this.categoryFilter.push(categoryFilter);
    });
  }

  performSearch(): void {
    let payload: SearchPayload = {
      query: this.searchForm.get('searchInput')?.value ?? '',
      distance: 1,
      limit: 50,
    };

    const tipoInputValue = this.searchForm.get('tipoInput')?.value;
    if (tipoInputValue) {
      payload.factTypeFilter = [tipoInputValue];
    }

    if (this.categoryFilter.length > 0) {
      payload.categoryFilter = this.categoryFilter;
    }

    this.knowledgeService.postQuery(payload).subscribe({
      next: (response) => {
        console.log(response);
        this.results = response;
      },
      error: (err) => {
        console.error('Error:', err);
      },
    });
  }

  clickOnResult(res: Fact) {
    this.selectedResult = res;
  }

  deleteFact(f: Fact) {
    this.openConfirmationDialog().subscribe((result) => {
      if (result === true) {
        this.knowledgeService.deleteFact(f.id).subscribe({
          next: (response) => {
            console.log(response);
            this.results = response;
            this.snackbarService.showSnackbar(
              'Elemento eliminato',
              'Chiudi',
              2000
            );
            this.selectedResult = null;
          },
          error: (err) => {
            console.error('Error:', err);
            this.snackbarService.showSnackbar('Errore', 'Chiudi', 2000);
          },
        });
      }
    });
  }

  openConfirmationDialog(): Observable<boolean> {
    const confirmationDialogData: ConfirmationDialogData = {
      title: 'Conferma eliminazione',
      content: 'Sei sicuro di voler procedere?',
      confirmButtonText: 'Sì',
      cancelButtonText: 'No',
    };

    const dialogRef: MatDialogRef<DialogConfirmComponent, boolean> =
      this.dialog.open(DialogConfirmComponent, {
        width: '300px',
        data: confirmationDialogData,
      });

    return dialogRef
      .afterClosed()
      .pipe(map((result) => (result === undefined ? false : result)));
  }

  openFactDialog(fact?: Fact) {
    this.knowledgeService.getSibyllaeConfigs().subscribe((response) => {
      const selectedSibylla = localStorage.getItem('selectedSibylla');
      const categoryFilter = response.find((c) => c.id == selectedSibylla)
        ?.memoryConfiguration.categoryFilter![0];
      const dialogRef = this.dialog.open(FactDialogComponent, {
        width: '500px',
        data: fact
          ? fact
          : { category: categoryFilter ? categoryFilter : selectedSibylla },
      });

      dialogRef.afterClosed().subscribe((result) => {
        console.log('The dialog was closed');
        // Handle any actions after the dialog is closed
        // log the result
        console.log(result);
        let fact!: Fact;
        // create new fact with result
        let formValue = result.formValue;
        if (formValue) {
          fact = {
            id: result.id ? result.id : '3fa85f64-5717-4562-b3fc-2c963f66afa6',
            title: formValue.title,
            factType: formValue.factType,
            category: formValue.category,
            expiration: formValue.expiration,
            tags: formValue.tags,
            content: formValue.content,
            citation: formValue.source,
            reference: '',
          };
        }
        if (result.action === 'add') {
          this.knowledgeService.postFact(fact).subscribe({
            next: (response) => {
              console.log(response);
              this.results = response;
              this.snackbarService.showSnackbar(
                'Elemento aggiunto',
                'Chiudi',
                2000
              );
            },
            error: (err) => {
              console.error('Error:', err);
              this.snackbarService.showSnackbar('Errore', 'Chiudi', 2000);
            },
          });
        } else if (result.action === 'edit') {
          this.knowledgeService.updateFact(fact).subscribe({
            next: (response: any) => {
              console.log(response);
              this.results = response;
              this.snackbarService.showSnackbar(
                'Elemento modificato',
                'Chiudi',
                2000
              );
            },
            error: (err: any) => {
              console.error('Error:', err);
              this.snackbarService.showSnackbar('Errore', 'Chiudi', 2000);
            },
          });
        } else if (result.action === 'delete') {
          this.knowledgeService.deleteFact(result.id).subscribe({
            next: (response: any) => {
              console.log(response);
              this.results = response;
              this.snackbarService.showSnackbar(
                'Elemento eliminato',
                'Chiudi',
                2000
              );
            },
            error: (err: any) => {
              console.error('Error:', err);
              this.snackbarService.showSnackbar('Errore', 'Chiudi', 2000);
            },
          });
        }
      });
    });
  }

  openImportDialog() {
    const dialogRef = this.dialog.open(ImportDialogComponent, {
      width: '500px',
      maxHeight: '90%',
      // You can also pass data or configuration here if needed
    });

    dialogRef.afterClosed().subscribe((result) => {
      console.log('The dialog was closed');
      // Handle any actions after the dialog is closed
    });
  }
}
