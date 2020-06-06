import { NgModule, Component, OnInit, OnDestroy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NotificationProvider } from '@providers';
import { AuthenticationService } from '@services';
import { GameEntity, NotificationType } from '@models';
import { GameQuestionsComponentModule } from '../game-questions/game-questions.component';

@Component({
    selector: 'app-game-completed',
    templateUrl: './game-completed.component.html'
})
export class GameCompletedComponent implements OnInit, OnDestroy {
    @Input() game: GameEntity;

    private componentDestroyed = new Subject<void>();

    constructor(private notificationProvider: NotificationProvider, private authenticationService: AuthenticationService) {
    }

    async ngOnInit(): Promise<void> {
        const userId = this.authenticationService.getAccessToken().userId;

        if (this.game.opponentId == userId) {
            this.notificationProvider.notificationsUpdated
                .pipe(
                    takeUntil(this.componentDestroyed)
                )
                .subscribe(async notifications => {
                    const gameNotification = notifications.find(n => n.type == NotificationType.Game && n.recordId == this.game.id);

                    if (gameNotification) {
                        await this.notificationProvider.removeNotification({
                            id: gameNotification.id
                        });
                    }
                });
        }
    }

    ngOnDestroy(): void {
        this.componentDestroyed.next();
        this.componentDestroyed.complete();
    }
}

@NgModule({
    imports: [CommonModule, GameQuestionsComponentModule],
    declarations: [GameCompletedComponent],
    exports: [GameCompletedComponent]
})
export class GameCompletedComponentModule { }