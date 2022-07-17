import {Component, Input, OnInit, Self} from '@angular/core';
import {ControlValueAccessor, NgControl} from "@angular/forms";
import {BsDatepickerConfig} from "ngx-bootstrap/datepicker";

@Component({
  selector: 'app-date-input',
  templateUrl: './date-input.component.html',
  styleUrls: ['./date-input.component.css']
})
export class DateInputComponent implements ControlValueAccessor {
  @Input() label!: string
  @Input() maxDate!: Date
  // When we use partial, what we are really saying here is that every single property inside this type is going to be optional,
  // we don't have to provide all the different configuration options. We can only apply, or we could only provide a couple of them,
  // if we didn't use partial, then we would have to provide every single possible configuration option.
  bsConfig?: Partial<BsDatepickerConfig>

  // @Self(): the dependencies are injected locally and it doesn't try and get this ngControl from somewhere else in the dependency tree.
  constructor(@Self() public ngControl: NgControl) {
    ngControl.valueAccessor = this
    this.bsConfig = {
      containerClass: 'theme-red',
      dateInputFormat: 'DD MMMM YYYY',
    }
  }

  registerOnChange(fn: any): void {
  }

  registerOnTouched(fn: any): void {
  }

  writeValue(obj: any): void {
  }



}
