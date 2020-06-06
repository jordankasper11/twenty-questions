import { NgModule, Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { UpdateSettingsRequest, UserEntity } from '@models';
import { UserService, AuthenticationService } from '@services';
import { FormProvider } from '@providers';
import { environment } from '@environments';
import { takeUntil } from 'rxjs/operators';

@Component({
    selector: 'app-settings',
    templateUrl: './settings.component.html'
})
export class SettingsComponent implements OnInit, OnDestroy {
    user: UserEntity;
    form: FormGroup;
    defaultAvatarUrl = environment.defaultAvatarUrl;
    errorMessage: string;
    
    private avatar: File;
    private removedAvatar = false;
    private componentDestroyed = new Subject<void>();

    constructor(
        private userService: UserService,
        private authenticationService: AuthenticationService,
        private router: Router,
        private route: ActivatedRoute
    ) { }

    async ngOnInit(): Promise<void> {
        await this.loadUser();
    }

    ngOnDestroy(): void {
        this.componentDestroyed.next();
        this.componentDestroyed.complete();
    }

    async loadUser(): Promise<void> {
        const userId = this.authenticationService.getAccessToken().userId;
        const user = await this.userService.get(userId).toPromise();

        this.buildForm(user);

        this.user = user;
    }

    buildForm(user: UserEntity): void {
        this.form = new FormGroup({
            username: new FormControl(user.username, { updateOn: 'blur', validators: [FormProvider.validators.requiredTrim], asyncValidators: [this.validateUsernameAvailablity.bind(this)] }),
            email: new FormControl(user.email, [Validators.required, Validators.email]),
            password: new FormControl('', [Validators.required]),
            newPassword: new FormControl('', [Validators.minLength(6)]),
            confirmNewPassword: new FormControl({ value: '', disabled: true }, [this.validateCompareNewPassword.bind(this)]),
            avatar: new FormControl('')
        });

        const newPassword = this.form.get('newPassword');
        const confirmNewPassword = this.form.get('confirmNewPassword');

        newPassword.valueChanges
            .pipe(
                takeUntil(this.componentDestroyed)
            )
            .subscribe(value => {
                if (value)
                    confirmNewPassword.enable();
                else
                    confirmNewPassword.disable();
            });
    }

    async validateUsernameAvailablity(control: AbstractControl): Promise<ValidationErrors> {
        const username = control.value;
        const userId = this.authenticationService.getAccessToken().userId;
        const usernameAvailable = await this.userService.getUsernameAvailability(username, userId).toPromise();

        if (!usernameAvailable)
            return { 'unavailable': true };

        return null;
    };

    validateCompareNewPassword(control: AbstractControl): ValidationErrors {
        if (this.form) {
            const newPassword = this.form.get('newPassword').value;
            const confirmNewPassword = control.value;

            if (newPassword && !confirmNewPassword)
                return { 'required': true };
            else if (newPassword != confirmNewPassword)
                return { 'passwordMatch': true };
        }

        return null;
    };

    removeAvatar(): void {
        this.user.avatarUrl = null;

        this.removedAvatar = true;
    }

    avatarChanged(event: Event) {
        const fileUpload = <HTMLInputElement>event.target;

        this.avatar = fileUpload.files.length ? fileUpload.files[0] : null;
    }

    async submit(): Promise<void> {
        if (this.form.valid) {
            this.errorMessage = null;

            try {
                const userId = this.authenticationService.getAccessToken().userId;
                const request = new UpdateSettingsRequest();

                request.userId = userId
                request.username = this.form.value.username;
                request.email = this.form.value.email;
                request.password = this.form.value.password;
                request.newPassword = this.form.value.newPassword ?this.form.value.newPassword : null;

                const user = await this.userService.updateSettings(request).toPromise();

                if (this.avatar)
                    await this.userService.saveAvatar(user.id, this.avatar).toPromise();
                else if (this.removedAvatar)
                    await this.userService.removeAvatar(userId).toPromise();

                this.router.navigate(['/']);
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
    declarations: [SettingsComponent],
    exports: [SettingsComponent]
})
export class SettingsComponentModule { }
