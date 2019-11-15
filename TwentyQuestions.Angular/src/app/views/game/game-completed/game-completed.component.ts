import { NgModule, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameEntity } from '@models';

@Component({
    selector: 'app-game-completed',
    templateUrl: './game-completed.component.html'
})
export class GameCompletedComponent {
    @Input() game: GameEntity;

    constructor() {}
}

@NgModule({
    imports: [CommonModule],
    declarations: [GameCompletedComponent],
    exports: [GameCompletedComponent]
})
export class GameCompletedComponentModule { }