import { NgModule, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { DurationPipeModule } from '@pipes';
import { FriendService, UserService } from '@services';
import { FriendRequest, FriendEntity, EntityStatus, UserEntity, UserRequest } from '@models';

@Component({
    selector: 'app-friends-search',
    templateUrl: './friends-search.component.html'
})
export class FriendsSearchComponent implements OnInit {
    form: FormGroup;
    users: Array<UserEntity>;
    searchCompleted = false;

    constructor(
        private userService: UserService,
        private friendService: FriendService,
        private router: Router
    ) { }

    ngOnInit(): void {
        this.buildForm();
    }

    buildForm() {
        this.form = new FormGroup({
            username: new FormControl('', [Validators.required])
        });
    }

    async submit(): Promise<void> {
        this.users = null;

        const request = new UserRequest();

        request.username = this.form.value.username;
        request.friendSearch = true;

        const response = await this.userService.query(request).toPromise();

        if (response)
            this.users = response.items;

        this.searchCompleted = true;
    }

    async addFriend(user: UserEntity): Promise<void> {
        let friend = new FriendEntity();

        friend.friendId = user.id;

        friend = await this.friendService.upsert(friend).toPromise();

        this.router.navigate(['/friends']);
    }
}

@NgModule({
    imports: [CommonModule, ReactiveFormsModule, RouterModule, DurationPipeModule],
    declarations: [FriendsSearchComponent],
    exports: [FriendsSearchComponent],
    providers: [UserService, FriendService]
})
export class FriendsSearchComponentModule { }
