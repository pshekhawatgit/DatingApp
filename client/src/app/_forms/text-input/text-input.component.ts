import { Component, Input, Self } from '@angular/core';
import { ControlValueAccessor, FormControl, NgControl } from '@angular/forms';

@Component({
  selector: 'app-text-input',
  templateUrl: './text-input.component.html',
  styleUrls: ['./text-input.component.css']
})
 // Prady added reference to ControlValueAccessor to create reusable component
export class TextInputComponent implements ControlValueAccessor {
  @Input() label = ''
  @Input() type = 'text'

// Injected Self decorator from Angular core, to avoid reusing controls from memory
  constructor(@Self() public ngControl: NgControl){
    this.ngControl.valueAccessor = this;
   }

  writeValue(obj: any): void {
  }

  registerOnChange(fn: any): void {
  }

  registerOnTouched(fn: any): void {
  }

  get control(): FormControl {
    return this.ngControl.control as FormControl
  }

}
