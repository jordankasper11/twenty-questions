import { BaseTrackedEntity, BaseRequest, BaseEntityRequest } from './base.models';

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
    public friendId: string;
    public friendUsername: string;
    public friendAvatarUrl: string;
    public maxQuestions: number;
    public completed: boolean;
    public questions: Array<QuestionEntity> = [];
}

export class GameRequest extends BaseEntityRequest<GameEntity> {
    userId: string;
    completed: boolean;
}

export class AskQuestionRequest extends BaseRequest {
    gameId: string;
    question: string;

    constructor(gameId: string) {
        super();

        this.gameId = gameId;
    }
}

export class AnswerQuestionRequest extends BaseRequest {
    gameId: string;
    questionId: number;
    response: QuestionResponse;
    responseExplanation: string;

    constructor(gameId: string, questionId: number) {
        super();
        
        this.gameId = gameId;
        this.questionId = questionId;
    }
}