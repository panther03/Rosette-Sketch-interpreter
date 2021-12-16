#lang rosette

(require
  rosette/lib/match
  rosette/lib/destruct
  rosette/lib/angelic
  rosette/lib/synthax)

(current-bitwidth #f)

(define (ite c x y) (if c x y))

(define-syntax-rule (set-vals-list __depth x y z val)
     (define-values (__depth x y z) (let ([l val]) (values (first l) (second l) (third l) (fourth l)))))

;;; STRUCT DECL SECTION

; Start nonterminal
(struct Struct_$while (b.t1 s.t1))
; S nonterminal
(struct Struct_$assign-x (e.t1))
; E nonterminal
(struct Struct_$1 ())
(struct Struct_$x ())
(struct Struct_$y ())
(struct Struct_$z ())
(struct Struct_$v ())
(struct Struct_$w ())
(struct Struct_$+ (e.t1 e.t2))
; B nonterminal
(struct Struct_$lt_x (e.t1))

;;; SYNTAX SECTION

(current-grammar-depth 3)
(define-grammar (gram)
  [Start
    (choose
      (Struct_$while (B) (S))
    )]
  [S
    (choose
      (Struct_$assign-x (E))
    )]
  [E
    (choose
      (Struct_$1)
      (Struct_$x)
      (Struct_$y)
      (Struct_$z)
      (Struct_$v)
      (Struct_$w)
      (Struct_$+ (E) (E))
    )]
  [B
    (choose
      (Struct_$lt_x (E))
    )]
)

;;; SEMANTICS SECTION

(define (Start.Sem  start.t __depth x0 y0 z0 v0 w0)
  (assert (>= __depth 0))
  (destruct start.t
    [(Struct_$while b.t1 s.t1) (begin (define b.v1 (B.Sem b.t1 x0 y0 z0 v0 w0)) (if (b.v1)
          (begin (define b.v1 (B.Sem b.t1 x0 y0 z0 v0 w0)) (set-vals-list  x1 y1 z1 v1 w1 (S.Sem s.t1 x0 y0 z0 v0 w0)) (set-vals-list __depth_o1  x2 y2 z2 v2 w2 (Start.Sem start.t __depth_o0 x1 y1 z1 v1 w1)) (list (- __depth_o1 1) x2))
          (begin (define y2 y0) (define z2 z0) (define v2 v0) (define w2 w0) (define b.v1 (B.Sem b.t1 x0 y0 z0 v0 w0)) (list (- __depth 1) x0))))]
  )
)
(define (S.Sem  s.t x0 y0 z0 v0 w0)
  (destruct s.t
    [(Struct_$assign-x e.t1) (begin (define e.v1 (E.Sem e.t1 x0 y0 z0 v0 w0)) (list e.v1 y0 z0 v0 w0))]
  )
)
(define (E.Sem  e.t x0 y0 z0 v0 w0)
  (destruct e.t
    [(Struct_$1)  1]
    [(Struct_$x)  x0]
    [(Struct_$y)  y0]
    [(Struct_$z)  z0]
    [(Struct_$v)  v0]
    [(Struct_$w)  w0]
    [(Struct_$+ e.t1 e.t2) (begin (define e.v1 (E.Sem e.t1 x0 y0 z0 v0 w0)) (define e.v2 (E.Sem e.t2 x0 y0 z0 v0 w0)) (+ e.v1 e.v2))]
  )
)
(define (B.Sem  b.t x0 y0 z0 v0 w0)
  (destruct b.t
    [(Struct_$lt_x e.t1) (begin (define e.v1 (E.Sem e.t1 x0 y0 z0 v0 w0)) (< x0 e.v1))]
  )
)

;;; CONSTRAINTS SECTION

(define (sol) (gram))
(define sol_Start
(synthesize
#:forall (list)
#:guarantee (assert (and (equal? (Start.Sem (sol) 4 5 1 3 2) 15) (equal? (Start.Sem (sol) 13 16 9 11 21) 70)))))
(print-forms sol_Start)
