import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import {MatPaginator} from '@angular/material/paginator';
import {MatTableDataSource} from '@angular/material/table';
import { ImportDialog } from './ImportDialog/import-dialog.component';
import { GlobalVariableService } from './service';


@Component({
  selector: 'app-kpi',
  templateUrl: './kpi.component.html',
  styleUrls: ['./kpi.component.scss']
})


export class KpiComponent implements OnInit , AfterViewInit{

  displayedColumns: string[] = ['Data', 'Faithfulness', 'Answerrelevancy', 'Contextprecision','Contextrecall',
  'Answersimilarity','Answercorrectness','Totalscore','Esporta'];
  dataSource = new MatTableDataSource<PeriodicElement>(ELEMENT_DATA);

  @ViewChild(MatPaginator) paginator!: MatPaginator; 
  @ViewChild(ImportDialog) importDialog!: ImportDialog;


  constructor(public dialog: MatDialog, private globalVariableService: GlobalVariableService) {
  }

  openDialog() {
    const dialogRef = this.dialog.open(ImportDialog);

    dialogRef.afterClosed().subscribe(result => {
      console.log(`Dialog result: ${result}`);
    });
  }

  ngOnInit() {
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.importDialog = new ImportDialog(this.globalVariableService,this.dialog);
  }

}

export interface PeriodicElement {
  Data: string;
  Faithfulness: number;
  Answerrelevancy : number;
  Contextprecision: number;
  Contextrecall: number,
  Answersimilarity: number,
  Answercorrectness: number,
  Totalscore: number,
  Esporta: string
}

const ELEMENT_DATA: PeriodicElement[] = [
  {Data: '22/12/2023', Faithfulness: 44.12, Answerrelevancy : 1.0079, Contextprecision: 33.33, Contextrecall: 33.33, Answersimilarity: 31.44,
   Answercorrectness:27.44, Totalscore: 77.33, Esporta: "In corso .."},
];
