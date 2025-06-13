import { Nullable } from 'primeng/ts-helpers';
import * as i0 from "@angular/core";
export declare class ContextMenuService {
    private activeItemKeyChange;
    activeItemKeyChange$: import("rxjs").Observable<string>;
    activeItemKey: Nullable<string>;
    changeKey(key: string): void;
    reset(): void;
    static ɵfac: i0.ɵɵFactoryDeclaration<ContextMenuService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<ContextMenuService>;
}
