import { Injectable } from '@angular/core';
import { AbstractControl, ValidationErrors } from '@angular/forms';

@Injectable()
export class FormProvider {
    public static validators = {
        requiredTrim: (control: AbstractControl): ValidationErrors => {
            const value = control.value.trim();

            if (!value)
                return { 'required': true };

            return null;
        }
    };
}
