import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {AccountService} from "../_services/account.service";
import {ToastrService} from "ngx-toastr";
import {AbstractControl, FormBuilder, FormGroup, ValidatorFn, Validators} from "@angular/forms";
import {Router} from "@angular/router";

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  // @Input() usersFromHomeComponent: any // Get the data from Home component/pass data from a parent component to a child component.
  @Output() cancelRegister = new EventEmitter() // pass data from a child component to a parent component and then we're emitting event.
  registerForm!: any // NOTE: EXAMINING!!!!!
  maxDate!: Date
  // Set this to an array of strings, because we know we're getting that back from our interceptor if there are such a thing
  // and we also need to initialize to empty array too because we're checking for the length of validationErrors in template file,
  // if we don't then we're going to get an error because that will then be undefined and undefined, doesn't have a length property.
  validationErrors: string[] = []

  constructor(
    private accountService: AccountService,
    private toastr: ToastrService,
    private fb: FormBuilder,
    private router: Router
  ) {
  }

  ngOnInit(): void {
    this.initializeForm()
    this.maxDate = new Date()
    // This will mean that our date picker doesn't allow any selection of dates less than 18 years ago.
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18)
  }

  // Initialize reactive form.
  initializeForm() {
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', [Validators.required, this.matchValues('password')]] // specify password as the field that we want to match this against.
    })
    // If we change the password after validate against confirm password, it will be valid, because the confirmed password validator is only applied to the confirmed password,
    // we need also check the password field again and update it is valid the T against the confirmed password.
    // Get the password control, check the value changes of this control then we subscribe to the value changes.
    this.registerForm.controls['password'].valueChanges.subscribe(() => {
      // So what we're going to be testing for is when our password changes, then we're going to update the validity of password against confirmPassword as well,
      // and if either one of them changes and does not match the other one, then we validate form once again.
      this.registerForm.controls['confirmPassword'].updateValueAndValidity()
    })
  }

  // Custom validator.
  // matchTo: string: because our field names are strings.
  // : -> specify a type of what we want to return and return ValidatorFn.
  matchValues(matchTo: string): ValidatorFn {
    // All the FormControl derive from an AbstractControl.
    return (control: AbstractControl) => {
      // control?.value: to confirm password control and compare this to whatever we pass into the matchTo and we're going to pass in the password,
      // that we want to compare this value to and if these passwords match, then we return null and that means validation has passed,
      // if the passwords don't match, then we attach a validator error called isMatching to the control, and then this will fail our form validation.
      // controls: gives us access to all of the controls in the form.
      // @ts-ignore
      return control?.value === control?.parent?.controls[matchTo].value ? null : {isMatching: true}
    }
  }

  register() {
    // This is going to contain the values for all of these FormControl().
    // console.log(this.registerForm.value)
    this.accountService.register(this.registerForm.value).subscribe({
      next: response => {
        this.router.navigateByUrl('/members')
      },
      error: error => {
        this.validationErrors = error
      }
    })
  }

  // When click cancel we want to emit a value.
  cancel() {
    this.cancelRegister.emit(false)
  }

}
