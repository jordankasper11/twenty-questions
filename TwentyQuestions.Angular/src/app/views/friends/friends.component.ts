import { NgModule, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DurationPipeModule } from '@pipes';
import { GameService, AuthenticationService, FriendService } from '@services';
import { FriendRequest, FriendEntity, EntityStatus } from '@models';
import { environment } from '@environments';

@Component({
    selector: 'app-friends',
    templateUrl: './friends.component.html'
})
export class FriendsComponent implements OnInit {
    userId: string;
    defaultAvatarUrl = environment.defaultAvatarUrl;
    invitations: Array<FriendEntity>;
    friends: Array<FriendEntity>;

    constructor(private friendService: FriendService, private authenticationService: AuthenticationService) { }

    async ngOnInit(): Promise<void> {
        this.userId = this.authenticationService.getAccessToken().userId;

        await this.loadFriends();
    }

    async loadFriends(): Promise<void> {
        const request = new FriendRequest();

        request.userId = this.authenticationService.getAccessToken().userId;
        request.status = EntityStatus.Active | EntityStatus.Pending;

        const response = await this.friendService.query(request).toPromise();
        const userId = this.authenticationService.getAccessToken().userId;

        if (response) {
            this.invitations = response.items.filter(g => g.status == EntityStatus.Pending);
            this.friends = response.items.filter(g => g.status == EntityStatus.Active);
        }
    }

    async onFriendAdded(): Promise<void> {
        await this.loadFriends();
    }

    async acceptInvitation(friend: FriendEntity): Promise<void> {
        await this.friendService.acceptInvitation(friend.id).toPromise();
        await this.loadFriends();
    }

    async declineInvitation(friend: FriendEntity): Promise<void> {
        await this.friendService.declineInvitation(friend.id).toPromise();
        await this.loadFriends();
    }

    async cancelInvitation(friend: FriendEntity): Promise<void> {
        await this.friendService.delete(friend.id).toPromise();
        await this.loadFriends();
    }

    async removeFriend(friend: FriendEntity): Promise<void> {
        await this.friendService.delete(friend.id).toPromise();
        await this.loadFriends();
    }
}

@NgModule({
    imports: [CommonModule, RouterModule, DurationPipeModule],
    declarations: [FriendsComponent],
    exports: [FriendsComponent],
    providers: [GameService, AuthenticationService]
})
export class FriendsComponentModule { }
