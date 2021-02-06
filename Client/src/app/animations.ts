import { trigger, state, style, transition, animate, keyframes } from '@angular/animations';

export const highlightAnimation = trigger("highlight", [
  state("1", style({
    background: "none"
  })),
  state("0", style({})),
  transition("1 <=> 0", [
    animate("0.5s", keyframes([
      style({ background: "var(--info)" })
    ])),
    animate("3.5s", keyframes([
      style({ background: "none" })
    ]))
  ])
])
