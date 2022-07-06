import {Component, Input, OnInit, Self} from '@angular/core';
import {ControlValueAccessor, NgControl} from "@angular/forms";

@Component({
  selector: 'app-text-input',
  templateUrl: './text-input.component.html',
  styleUrls: ['./text-input.component.css']
})

// We want to implement a control value accessor.
export class TextInputComponent implements ControlValueAccessor {
  @Input() label!: string
  // Give this an initial property of text and that's going to be its default,
  // and if we want to override this, then we'll simply pass in a type input property to our control as well.
  @Input() type = 'text'

  // Inject the control into the constructor of this component.
  // What Angular will do when it's looking at dependency injection is going to look inside the hierarchy of things that it can inject,
  // if there's an injector that matches this to this already got inside its dependency injection container,
  // then it's going to attempt to reuse that one. We don't want that to happen for this. We want our text input components
  // to be self-contained and we don't want Angular to try and fetch us another instance of what we're doing here. We always want this to be self-contained,
  // snd this @Self() decorator ensures that Angular will always inject what we're doing here locally into this component.
  // NgControl is base class that FormControl directives extend.
  constructor(@Self() public ngControl: NgControl) {
    // By adding 'this', now we've got access to our control inside this component when we use it inside of register form.
    ngControl.valueAccessor = this
  }

  // We don't to need to modify all this method because is just method needed by ControlValueAccessor.
  registerOnChange(fn: any): void {
  }

  registerOnTouched(fn: any): void {
  }

  writeValue(obj: any): void {
  }



}
