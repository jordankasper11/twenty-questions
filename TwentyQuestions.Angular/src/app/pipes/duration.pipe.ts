import { NgModule, Pipe, PipeTransform } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';

@Pipe({
    name: 'duration',
    pure: false
})
export class DurationPipe implements PipeTransform {
    transform(value: Date): string {
        if (value) {
            if (!(value instanceof Date))
                throw new Error("Value must be a date");

            let duration = (new Date().getTime() - value.getTime()) / 1000;

            if (duration < 60)
                return `less than a minute ago`;
            else if ((duration / 60) < 120) {
                duration = Math.floor(duration / 60);

                return `${duration} ${duration == 1 ? 'minute' : 'minutes'} ago`;
            }
            else if ((duration / 60 / 60) < 48) {
                duration = Math.floor(duration / 60 / 60);

                return `${duration} ${duration == 1 ? 'hour' : 'hours'} ago`;
            }
            else if ((duration / 60 / 60 / 24) < 7) {
                duration = Math.floor(duration / 60 / 60 / 24);

                return `${duration} ${duration == 1 ? 'day' : 'days'} ago`;
            }
            else {
                const datePipe = new DatePipe(navigator.language);

                return datePipe.transform(value, "shortDate");
            }
        }

        return null;
    }
}

@NgModule({
    imports: [CommonModule],
    declarations: [DurationPipe],
    exports: [DurationPipe]
})
export class DurationPipeModule { }