import { Component, Inject } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  FormArray,
  FormControl,
} from '@angular/forms';
import { MatChipInputEvent } from '@angular/material/chips';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Fact } from 'src/app/interfaces/fact';
import language from 'src/app/config/language';

@Component({
  selector: 'app-fact-dialog',
  templateUrl: './fact-dialog.component.html',
  styleUrls: ['./fact-dialog.component.scss'], // Make sure to create this CSS file
})
export class FactDialogComponent {
  language = language;
  factForm!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private dialog: MatDialogRef<FactDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Fact
  ) {
    console.log(data);
    this.createForm();
  }

  createForm() {
    this.factForm = this.fb.group({
      title: [this.data ? this.data.title : '', [Validators.maxLength(100)]],
      factType: ['FAQ', Validators.required],
      category: [this.data ? this.data.category : '', Validators.required],
      //expiration: [this.data ? this.data.expiration : null],
      //tags: this.fb.array([]),
      //source: [this.data ? this.data.citation : ''],
      content: [
        this.data ? this.data.content : '',
        [Validators.required, Validators.minLength(10)],
      ],
    });
  }

  // Form control for the keywords (tags)
  formControl = new FormControl([]);
  keywords: string[] = []; // Initialize this with any existing keywords if necessary

  // Function to remove a keyword
  removeKeyword(keyword: string): void {
    const index = this.keywords.indexOf(keyword);
    if (index >= 0) {
      this.keywords.splice(index, 1);
    }
  }

  // Function to add a keyword
  add(event: any): void {
    const value = (event.value || '').trim();

    // Add the keyword if it is not empty
    if (value) {
      this.keywords.push(value);
    }

    // Clear the input value
    if (event.input) {
      event.input.value = '';
    }
  }

  get title() {
    return this.factForm.get('title');
  }
  get factType() {
    return this.factForm.get('factType');
  }
  get category() {
    return this.factForm.get('category');
  }
  get expiration() {
    return this.factForm.get('expiration');
  }
  get tags() {
    return this.factForm.get('tags') as FormArray;
  }
  get source() {
    return this.factForm.get('source');
  }
  get content() {
    return this.factForm.get('content');
  }

  onSubmit() {
    if (this.factForm.valid) {
      console.log(this.factForm.value);
      // send the data to modal parent
      this.dialog.close({
        formValue: this.factForm.value,
        action: 'add',
      });
    } else {
      // Trigger validation for all form fields
      this.factForm.markAllAsTouched();
    }
  }
  onDelete() {
    this.dialog.close({
      formValue: this.factForm.value,
      id: this.data.id,
      action: 'delete',
    });
  }
  onEdit() {
    if (this.factForm.valid) {
      console.log(this.factForm.value);
      // send the data to modal parent
      this.dialog.close({
        formValue: this.factForm.value,
        id: this.data.id,
        action: 'edit',
      });
    } else {
      // Trigger validation for all form fields
      this.factForm.markAllAsTouched();
    }
  }

  // Tag management logic can be added here
}
