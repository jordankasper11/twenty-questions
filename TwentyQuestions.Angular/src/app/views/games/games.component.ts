import { NgModule, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameService } from '@services';
import { GameRequest } from '@models';

@Component({
    selector: 'app-games',
    templateUrl: './games.component.html'
})
export class GamesComponent implements OnInit {
    constructor(private gameService: GameService) { }

    async ngOnInit(): Promise<void> {
        await this.load();
    }

    async load(): Promise<void> {
        const request = new GameRequest();

        await this.gameService.query(request).toPromise();
    }
}

@NgModule({
    imports: [CommonModule],
    declarations: [GamesComponent],
    exports: [GamesComponent]
})
export class GamesComponentModule { }
