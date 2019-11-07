import { BaseEntity } from './base.models';

export enum QuestionResponse {
    Yes,
    No,
    Sometimes,
    Probably,
    ProbablyNot,
    Correct,
    GameOver
}

export class QuestionEntity {
    public id: number;
    public question: string;
    public response: QuestionResponse;
    public responseExplanation: string;
    public createdDate: Date;
}

export class GameEntity extends BaseEntity {
    public creatorId: string;
    public challengerId: string;
    public maxQuestions: number;
    public questions: Array<QuestionEntity> = [];
}