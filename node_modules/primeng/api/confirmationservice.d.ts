import { Confirmation } from './confirmation';
import * as i0 from "@angular/core";
/**
 * Methods used in confirmation service.
 * @group Service
 */
export declare class ConfirmationService {
    private requireConfirmationSource;
    private acceptConfirmationSource;
    requireConfirmation$: import("rxjs").Observable<Confirmation>;
    accept: import("rxjs").Observable<Confirmation>;
    /**
     * Callback to invoke on confirm.
     * @param {Confirmation} confirmation - Represents a confirmation dialog configuration.
     * @group Method
     */
    confirm(confirmation: Confirmation): this;
    /**
     * Closes the dialog.
     * @group Method
     */
    close(): this;
    /**
     * Accepts the dialog.
     * @group Method
     */
    onAccept(): void;
    static ɵfac: i0.ɵɵFactoryDeclaration<ConfirmationService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<ConfirmationService>;
}
