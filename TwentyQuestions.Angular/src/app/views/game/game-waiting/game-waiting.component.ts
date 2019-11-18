import { NgModule, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameEntity } from '@models';
import { GameQuestionsComponentModule } from '../game-questions/game-questions.component';

@Component({
    selector: 'app-game-waiting',
    templateUrl: './game-waiting.component.html'
})
export class GameWaitingComponent {
    @Input() game: GameEntity;

    constructor() { }
}

@NgModule({
    imports: [CommonModule, GameQuestionsComponentModule],
    declarations: [GameWaitingComponent],
    exports: [GameWaitingComponent]
})
export class GameWaitingComponentModule { }