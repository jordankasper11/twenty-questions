import { NgModule, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators, ValidatorFn, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { RegistrationRequest } from '@models';
import { UserService } from '@services';
import { FormProvider } from '@providers';

@Component({
    selector: 'app-registration',
    templateUrl: './registration.component.html'
})
export class RegistrationComponent implements OnInit {
    form: FormGroup;
    errorMessage: string;

    constructor(
        private userService: UserService,
        private router: Router,
        private route: ActivatedRoute
    ) { }

    ngOnInit(): void {
        this.buildForm();
    }

    buildForm(): void {
        this.form = new FormGroup({
            username: new FormControl('', { updateOn: 'blur', validators: [FormProvider.validators.requiredTrim], asyncValidators: [this.validateUsernameAvailablity.bind(this)] }),
            email: new FormControl('', [Validators.required, Validators.email]),
            password: new FormControl('', [Validators.required, Validators.minLength(6)]),
            confirmPassword: new FormControl('', [Validators.required, this.validateComparePassword.bind(this)])
        });
    }

    async validateUsernameAvailablity(control: AbstractControl): Promise<ValidationErrors> {
        const username = control.value;
        const usernameAvailable = await this.userService.getUsernameAvailability(username).toPromise();

        if (!usernameAvailable)
            return { 'unavailable': true };

        return null;
    };

    validateComparePassword(control: AbstractControl): ValidationErrors {
        if (this.form) {
            const password = this.form.get('password').value;
            const confirmPassword = control.value;

            if (password != confirmPassword)
                return { 'passwordMatch': true };
        }

        return null;
    };

    async submit(): Promise<void> {
        if (this.form.valid) {
            this.errorMessage = null;

            const request = new RegistrationRequest();

            request.username = this.form.value.username;
            request.email = this.form.value.email;
            request.password = this.form.value.password;

            try {
                const user = await this.userService.register(request).toPromise();

                if (user)
                    this.router.navigate(['/login']);
            } catch (error) {
                if (error.error && error.status)
                    this.errorMessage = error.error;
                else
                    this.errorMessage = 'An error has occurred. Please try again.';
            }
        }
    }
}

@NgModule({
    imports: [CommonModule, ReactiveFormsModule],
    declarations: [RegistrationComponent],
    exports: [RegistrationComponent],
    providers: [UserService]
})
export class RegistrationComponentModule { }
