#lang rosette

(require
  rosette/lib/match
  rosette/lib/destruct
  rosette/lib/angelic
  rosette/lib/synthax)

(current-bitwidth #f)

; hack because i can't get the interpreter to generate #t and #f
(define True #t)
(define False #f)
(define (ite c x y) (if c x y))

(define-syntax-rule (set-vals-list __depth x y z val)
     (define-values (__depth x y z) (let ([l val]) (values (first l) (second l) (third l) (fourth l)))))

;;; STRUCT DECL SECTION

; Start nonterminal
(struct Struct_$eval (r.t))
; R nonterminal
(struct Struct_$eps ())
(struct Struct_$any ())
(struct Struct_$0 ())
(struct Struct_$1 ())
(struct Struct_$or (r.t1 r.t2))
(struct Struct_$concat (r.t1 r.t2))
(struct Struct_$star (r.t1))

;;; SYNTAX SECTION

(current-grammar-depth 3)
(define-grammar (gram)
  [Start
    (choose
      (Struct_$eval (R))
    )]
  [R
    (choose
      (Struct_$eps)
      (Struct_$any)
      (Struct_$0)
      (Struct_$1)
      (Struct_$or (R) (R))
      (Struct_$concat (R) (R))
      (Struct_$star (R))
    )]
)

;;; SEMANTICS SECTION

(define (Start.Sem  start.t len s_0 s_1 s_2 s_3 s_4 s_5)
  (destruct start.t
    [(Struct_$eval r.t) UNIMPLEMENTED]
  )
)
(define (R.Sem  r.t __depth len s_0 s_1 s_2 s_3 s_4 s_5)
  (assert (>= __depth 0))
  (destruct r.t
    [(Struct_$eps) (begin (list (- __depth 1) true false false false false false false true false false false false false true false false false false true false false false true false false true false true))]
    [(Struct_$any) (begin (list (- __depth 1) false true false false false false false false true false false false false false true false false false false true false false false true false false true false))]
    [(Struct_$0) (begin (list (- __depth 1) false (or (= s_0 0) (= s_0 3)) false false false false false false (or (= s_1 0) (= s_1 3)) false false false false false (or (= s_2 0) (= s_2 3)) false false false false (or (= s_3 0) (= s_3 3)) false false false (or (= s_4 0) (= s_4 3)) false false (or (= s_5 0) (= s_5 3)) false))]
    [(Struct_$1) (begin (list (- __depth 1) false (or (= s_0 1) (= s_0 3)) false false false false false false (or (= s_1 1) (= s_1 3)) false false false false false (or (= s_2 1) (= s_2 3)) false false false false (or (= s_3 1) (= s_3 3)) false false false (or (= s_4 1) (= s_4 3)) false false (or (= s_5 1) (= s_5 3)) false))]
    [(Struct_$or r.t1 r.t2) (begin (set-vals-list __depth_o0  A_0_0 A_0_1 A_0_2 A_0_3 A_0_4 A_0_5 A_0_6 A_1_1 A_1_2 A_1_3 A_1_4 A_1_5 A_1_6 A_2_2 A_2_3 A_2_4 A_2_5 A_2_6 A_3_3 A_3_4 A_3_5 A_3_6 A_4_4 A_4_5 A_4_6 A_5_5 A_5_6 A_6_6 (R.Sem r.t1 __depth len s_0 s_1 s_2 s_3 s_4 s_5)) (set-vals-list __depth_o1  B_0_0 B_0_1 B_0_2 B_0_3 B_0_4 B_0_5 B_0_6 B_1_1 B_1_2 B_1_3 B_1_4 B_1_5 B_1_6 B_2_2 B_2_3 B_2_4 B_2_5 B_2_6 B_3_3 B_3_4 B_3_5 B_3_6 B_4_4 B_4_5 B_4_6 B_5_5 B_5_6 B_6_6 (R.Sem r.t2 __depth_o0 len s_0 s_1 s_2 s_3 s_4 s_5)) (list (- __depth_o1 1) (or A_0_0 B_0_0) (or A_0_1 B_0_1) (or A_0_2 B_0_2) (or A_0_3 B_0_3) (or A_0_4 B_0_4) (or A_0_5 B_0_5) (or A_0_6 B_0_6) (or A_1_1 B_1_1) (or A_1_2 B_1_2) (or A_1_3 B_1_3) (or A_1_4 B_1_4) (or A_1_5 B_1_5) (or A_1_6 B_1_6) (or A_2_2 B_2_2) (or A_2_3 B_2_3) (or A_2_4 B_2_4) (or A_2_5 B_2_5) (or A_2_6 B_2_6) (or A_3_3 B_3_3) (or A_3_4 B_3_4) (or A_3_5 B_3_5) (or A_3_6 B_3_6) (or A_4_4 B_4_4) (or A_4_5 B_4_5) (or A_4_6 B_4_6) (or A_5_5 B_5_5) (or A_5_6 B_5_6) (or A_6_6 B_6_6)))]
    [(Struct_$concat r.t1 r.t2) (begin (set-vals-list __depth_o0  A_0_0 A_0_1 A_0_2 A_0_3 A_0_4 A_0_5 A_0_6 A_1_1 A_1_2 A_1_3 A_1_4 A_1_5 A_1_6 A_2_2 A_2_3 A_2_4 A_2_5 A_2_6 A_3_3 A_3_4 A_3_5 A_3_6 A_4_4 A_4_5 A_4_6 A_5_5 A_5_6 A_6_6 (R.Sem r.t1 __depth len s_0 s_1 s_2 s_3 s_4 s_5)) (set-vals-list __depth_o1  B_0_0 B_0_1 B_0_2 B_0_3 B_0_4 B_0_5 B_0_6 B_1_1 B_1_2 B_1_3 B_1_4 B_1_5 B_1_6 B_2_2 B_2_3 B_2_4 B_2_5 B_2_6 B_3_3 B_3_4 B_3_5 B_3_6 B_4_4 B_4_5 B_4_6 B_5_5 B_5_6 B_6_6 (R.Sem r.t2 __depth_o0 len s_0 s_1 s_2 s_3 s_4 s_5)) (list (- __depth_o1 1) (or (and A_0_0 B_0_0)) (or (and A_0_0 B_0_1) (and A_0_1 B_1_1)) (or (and A_0_0 B_0_2) (and A_0_1 B_1_2) (and A_0_2 B_2_2)) (or (and A_0_0 B_0_3) (and A_0_1 B_1_3) (and A_0_2 B_2_3) (and A_0_3 B_3_3)) (or (and A_0_0 B_0_4) (and A_0_1 B_1_4) (and A_0_2 B_2_4) (and A_0_3 B_3_4) (and A_0_4 B_4_4)) (or (and A_0_0 B_0_5) (and A_0_1 B_1_5) (and A_0_2 B_2_5) (and A_0_3 B_3_5) (and A_0_4 B_4_5) (and A_0_5 B_5_5)) (or (and A_0_0 B_0_6) (and A_0_1 B_1_6) (and A_0_2 B_2_6) (and A_0_3 B_3_6) (and A_0_4 B_4_6) (and A_0_5 B_5_6) (and A_0_6 B_6_6)) (or (and A_1_1 B_1_1)) (or (and A_1_1 B_1_2) (and A_1_2 B_2_2)) (or (and A_1_1 B_1_3) (and A_1_2 B_2_3) (and A_1_3 B_3_3)) (or (and A_1_1 B_1_4) (and A_1_2 B_2_4) (and A_1_3 B_3_4) (and A_1_4 B_4_4)) (or (and A_1_1 B_1_5) (and A_1_2 B_2_5) (and A_1_3 B_3_5) (and A_1_4 B_4_5) (and A_1_5 B_5_5)) (or (and A_1_1 B_1_6) (and A_1_2 B_2_6) (and A_1_3 B_3_6) (and A_1_4 B_4_6) (and A_1_5 B_5_6) (and A_1_6 B_6_6)) (or (and A_2_2 B_2_2)) (or (and A_2_2 B_2_3) (and A_2_3 B_3_3)) (or (and A_2_2 B_2_4) (and A_2_3 B_3_4) (and A_2_4 B_4_4)) (or (and A_2_2 B_2_5) (and A_2_3 B_3_5) (and A_2_4 B_4_5) (and A_2_5 B_5_5)) (or (and A_2_2 B_2_6) (and A_2_3 B_3_6) (and A_2_4 B_4_6) (and A_2_5 B_5_6) (and A_2_6 B_6_6)) (or (and A_3_3 B_3_3)) (or (and A_3_3 B_3_4) (and A_3_4 B_4_4)) (or (and A_3_3 B_3_5) (and A_3_4 B_4_5) (and A_3_5 B_5_5)) (or (and A_3_3 B_3_6) (and A_3_4 B_4_6) (and A_3_5 B_5_6) (and A_3_6 B_6_6)) (or (and A_4_4 B_4_4)) (or (and A_4_4 B_4_5) (and A_4_5 B_5_5)) (or (and A_4_4 B_4_6) (and A_4_5 B_5_6) (and A_4_6 B_6_6)) (or (and A_5_5 B_5_5)) (or (and A_5_5 B_5_6) (and A_5_6 B_6_6)) (or (and A_6_6 B_6_6))))]
    [(Struct_$star r.t1) (begin (if (NO_COND)
          (begin (list (- __depth 1) true false false false false false false true false false false false false true false false false false true false false false true false false true false true))
          (begin (define nxt (- len 1)) (set-vals-list __depth_o0  A_0_0 A_0_1 A_0_2 A_0_3 A_0_4 A_0_5 A_0_6 A_1_1 A_1_2 A_1_3 A_1_4 A_1_5 A_1_6 A_2_2 A_2_3 A_2_4 A_2_5 A_2_6 A_3_3 A_3_4 A_3_5 A_3_6 A_4_4 A_4_5 A_4_6 A_5_5 A_5_6 A_6_6 (R.Sem r.t1 __depth len s_0 s_1 s_2 s_3 s_4 s_5)) (set-vals-list __depth_o1  B_0_0 B_0_1 B_0_2 B_0_3 B_0_4 B_0_5 B_0_6 B_1_1 B_1_2 B_1_3 B_1_4 B_1_5 B_1_6 B_2_2 B_2_3 B_2_4 B_2_5 B_2_6 B_3_3 B_3_4 B_3_5 B_3_6 B_4_4 B_4_5 B_4_6 B_5_5 B_5_6 B_6_6 (R.Sem r.t __depth_o0 nxt s_0 s_1 s_2 s_3 s_4 s_5)) (list (- __depth_o1 1) (or B_0_0 (and A_0_0 B_0_0)) (or B_0_1 (and A_0_0 B_0_1) (and A_0_1 B_1_1)) (or B_0_2 (and A_0_0 B_0_2) (and A_0_1 B_1_2) (and A_0_2 B_2_2)) (or B_0_3 (and A_0_0 B_0_3) (and A_0_1 B_1_3) (and A_0_2 B_2_3) (and A_0_3 B_3_3)) (or B_0_4 (and A_0_0 B_0_4) (and A_0_1 B_1_4) (and A_0_2 B_2_4) (and A_0_3 B_3_4) (and A_0_4 B_4_4)) (or B_0_5 (and A_0_0 B_0_5) (and A_0_1 B_1_5) (and A_0_2 B_2_5) (and A_0_3 B_3_5) (and A_0_4 B_4_5) (and A_0_5 B_5_5)) (or B_0_6 (and A_0_0 B_0_6) (and A_0_1 B_1_6) (and A_0_2 B_2_6) (and A_0_3 B_3_6) (and A_0_4 B_4_6) (and A_0_5 B_5_6) (and A_0_6 B_6_6)) (or B_1_1 (and A_1_1 B_1_1)) (or B_1_2 (and A_1_1 B_1_2) (and A_1_2 B_2_2)) (or B_1_3 (and A_1_1 B_1_3) (and A_1_2 B_2_3) (and A_1_3 B_3_3)) (or B_1_4 (and A_1_1 B_1_4) (and A_1_2 B_2_4) (and A_1_3 B_3_4) (and A_1_4 B_4_4)) (or B_1_5 (and A_1_1 B_1_5) (and A_1_2 B_2_5) (and A_1_3 B_3_5) (and A_1_4 B_4_5) (and A_1_5 B_5_5)) (or B_1_6 (and A_1_1 B_1_6) (and A_1_2 B_2_6) (and A_1_3 B_3_6) (and A_1_4 B_4_6) (and A_1_5 B_5_6) (and A_1_6 B_6_6)) (or B_2_2 (and A_2_2 B_2_2)) (or B_2_3 (and A_2_2 B_2_3) (and A_2_3 B_3_3)) (or B_2_4 (and A_2_2 B_2_4) (and A_2_3 B_3_4) (and A_2_4 B_4_4)) (or B_2_5 (and A_2_2 B_2_5) (and A_2_3 B_3_5) (and A_2_4 B_4_5) (and A_2_5 B_5_5)) (or B_2_6 (and A_2_2 B_2_6) (and A_2_3 B_3_6) (and A_2_4 B_4_6) (and A_2_5 B_5_6) (and A_2_6 B_6_6)) (or B_3_3 (and A_3_3 B_3_3)) (or B_3_4 (and A_3_3 B_3_4) (and A_3_4 B_4_4)) (or B_3_5 (and A_3_3 B_3_5) (and A_3_4 B_4_5) (and A_3_5 B_5_5)) (or B_3_6 (and A_3_3 B_3_6) (and A_3_4 B_4_6) (and A_3_5 B_5_6) (and A_3_6 B_6_6)) (or B_4_4 (and A_4_4 B_4_4)) (or B_4_5 (and A_4_4 B_4_5) (and A_4_5 B_5_5)) (or B_4_6 (and A_4_4 B_4_6) (and A_4_5 B_5_6) (and A_4_6 B_6_6)) (or B_5_5 (and A_5_5 B_5_5)) (or B_5_6 (and A_5_5 B_5_6) (and A_5_6 B_6_6)) (or B_6_6 (and A_6_6 B_6_6))))))]
  )
)

;;; CONSTRAINTS SECTION

(define (sol) (gram))
(define sol_Start
(synthesize
#:forall (list)
#:guarantee (assert (and (equal? (Start.Sem (sol) 1 1 9 9 9 9 9) True) (equal? (Start.Sem (sol) 3 1 1 1 9 9 9) True) (equal? (Start.Sem (sol) 2 0 1 9 9 9 9) True) (equal? (Start.Sem (sol) 3 1 0 1 9 9 9) True) (equal? (Start.Sem (sol) 3 0 1 1 9 9 9) True) (equal? (Start.Sem (sol) 4 0 1 0 1 9 9) True) (equal? (Start.Sem (sol) 5 0 1 1 0 1 9) True) (equal? (Start.Sem (sol) 1 0 9 9 9 9 9) False) (equal? (Start.Sem (sol) 3 0 0 1 9 9 9) False) (equal? (Start.Sem (sol) 2 1 0 9 9 9 9) False) (equal? (Start.Sem (sol) 4 1 0 1 0 9 9) False) (equal? (Start.Sem (sol) 5 0 1 0 1 0 9) False) (equal? (Start.Sem (sol) 6 0 1 1 0 1 0) False)))))
(print-forms sol_Start)
