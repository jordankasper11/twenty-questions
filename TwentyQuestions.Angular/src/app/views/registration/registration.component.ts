import { NgModule, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { RegistrationRequest } from '@models';
import { UserService } from '@services';

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
            username: new FormControl('', [Validators.required]),
            email: new FormControl('', [Validators.required]),
            password: new FormControl('', [Validators.required])
        });
    }

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
    exports: [RegistrationComponent]
})
export class RegistrationComponentModule { }
