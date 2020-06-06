export enum NotificationType
{
    Friend,
    Game
}

export class NotificationEntity {
    public id: string;
    public userId: string;
    public createdDate: Date;
    public type: NotificationType;
    public recordId?: string;
}