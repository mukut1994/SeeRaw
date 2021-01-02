export class RenderTarget {
    name: string;
    private value: any;

    // public RenderTarget(name: string, value: any) {
    //     this.name = name;
    //     this.value = value;
    // }
    
    public get Value() {
        return this.value;
    }

    public set Value(value: any) {
        this.value = value;
    }
}