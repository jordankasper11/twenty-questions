import { BaseTrackedEntity, BaseEntityRequest } from './base.models';

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

export class GameEntity extends BaseTrackedEntity {
    public subject: string;
    public opponentId: string;
    public maxQuestions: number;
    public completed: boolean;
    public questions: Array<QuestionEntity> = [];
}

export class GameRequest extends BaseEntityRequest<GameEntity> {
}