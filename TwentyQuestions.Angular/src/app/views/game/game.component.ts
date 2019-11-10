import { NgModule, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
    ReactiveFormsModule,
    FormControl,
    FormGroup,
    Validators
} from '@angular/forms';
import { GameService, FriendService } from '@services';
import { GameRequest, FriendRequest, FriendEntity } from '@models';

@Component({
    selector: 'app-game',
    templateUrl: './game.component.html'
})
export class GameComponent implements OnInit {
    form: FormGroup;
    friends: Array<FriendEntity>;

    constructor(private gameService: GameService, private friendService: FriendService) { }

    async ngOnInit(): Promise<void> {
        this.buildForm();
        await this.loadFriends();
    }

    buildForm() {
        this.form = new FormGroup({
            subject: new FormControl('', [Validators.required]),
            opponentId: new FormControl('', [Validators.required])
        });
    }

    async loadFriends(): Promise<void> {
        const request = new FriendRequest();
        const response = await this.friendService.query(request).toPromise();

        this.friends = response.items; 
    }

    async submit() {
        if (this.form.valid) {

        }
    }
}

@NgModule({
    imports: [CommonModule, ReactiveFormsModule],
    declarations: [GameComponent],
    exports: [GameComponent]
})
export class GameComponentModule { }
