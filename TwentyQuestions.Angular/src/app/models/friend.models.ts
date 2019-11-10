import { BaseTrackedEntity, BaseEntityRequest } from './base.models';

export class FriendEntity extends BaseTrackedEntity {
    public friendId: string;
    public username: string;
}

export class FriendRequest extends BaseEntityRequest<FriendEntity> {
    public userId: string;
}