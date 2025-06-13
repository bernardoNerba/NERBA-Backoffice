import { Observable, Subject } from 'rxjs';
/**
 * Dynamic Dialog instance.
 * @group Components
 */
export declare class DynamicDialogRef<ComponentType = any> {
    constructor();
    /**
     * Closes dialog.
     * @group Method
     */
    close(result?: any): void;
    /**
     * Destroys the dialog instance.
     * @group Method
     */
    destroy(): void;
    /**
     * Callback to invoke on drag start.
     * @param {MouseEvent} event - Mouse event.
     * @group Method
     */
    dragStart(event: MouseEvent): void;
    /**
     * Callback to invoke on drag end.
     * @param {MouseEvent} event - Mouse event.
     * @group Method
     */
    dragEnd(event: MouseEvent): void;
    /**
     * Callback to invoke on resize start.
     * @param {MouseEvent} event - Mouse event.
     * @group Method
     */
    resizeInit(event: MouseEvent): void;
    /**
     * Callback to invoke on resize start.
     * @param {MouseEvent} event - Mouse event.
     * @group Method
     */
    resizeEnd(event: MouseEvent): void;
    /**
     * Callback to invoke on dialog is maximized.
     * @param {*} value - Size value.
     * @group Method
     */
    maximize(value: any): void;
    private readonly _onClose;
    /**
     * Event triggered on dialog is closed.
     * @group Events
     */
    onClose: Observable<any>;
    private readonly _onDestroy;
    /**
     * Event triggered on dialog instance is destroyed.
     * @group Events
     */
    onDestroy: Observable<any>;
    private readonly _onDragStart;
    /**
     * Event triggered on drag start.
     * @param {MouseEvent} event - Mouse event.
     * @group Events
     */
    onDragStart: Observable<any>;
    private readonly _onDragEnd;
    /**
     * Event triggered on drag end.
     * @param {MouseEvent} event - Mouse event.
     * @group Events
     */
    onDragEnd: Observable<any>;
    private readonly _onResizeInit;
    /**
     * Event triggered on resize start.
     * @param {MouseEvent} event - Mouse event.
     * @group Events
     */
    onResizeInit: Observable<any>;
    private readonly _onResizeEnd;
    /**
     * Event triggered on resize end.
     * @param {MouseEvent} event - Mouse event.
     * @group Events
     */
    onResizeEnd: Observable<any>;
    private readonly _onMaximize;
    /**
     * Event triggered on dialog is maximized.
     * @param {*} value - Size value.
     * @group Events
     */
    onMaximize: Observable<any>;
    /**
     * Event triggered on child component load.
     * @param {*} value - Chi.
     * @group Events
     */
    readonly onChildComponentLoaded: Subject<ComponentType>;
}
