import { NgModule, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
    ReactiveFormsModule,
    FormControl,
    FormGroup,
    Validators
} from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { LoginRequest, AccessToken } from '@models';
import { AuthenticationService } from '@services';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {
    form: FormGroup;
    errorMessage: string;

    constructor(
        private authenticationService: AuthenticationService,
        private router: Router,
        private route: ActivatedRoute
    ) { }

    ngOnInit(): void {
        this.buildForm();
    }

    buildForm(): void {
        this.form = new FormGroup({
            username: new FormControl('', [Validators.required]),
            password: new FormControl('', [Validators.required])
        });
    }

    async submit(): Promise<void> {
        if (this.form.valid) {
            this.errorMessage = null;

            const loginRequest: LoginRequest = new LoginRequest(
                this.form.value.username,
                this.form.value.password
            );

            try {
                const accessToken = await this.authenticationService
                    .login(loginRequest)
                    .toPromise();

                if (accessToken) {
                    const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';

                    this.router.navigateByUrl(returnUrl);
                }
            } catch (error) {
                if (error.error && error.status)
                    this.errorMessage = error.error;
                else if (error.status == 401)
                    this.errorMessage = 'Invalid credentials';
                else
                    this.errorMessage = 'An error has occurred. Please try again.';
            }
        }
    }
}

@NgModule({
    imports: [CommonModule, ReactiveFormsModule],
    declarations: [LoginComponent],
    exports: [LoginComponent]
})
export class LoginComponentModule { }
