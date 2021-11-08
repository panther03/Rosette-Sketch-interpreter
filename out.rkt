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

;;; STRUCT DECL SECTION

; Start nonterminal
(struct Struct_$eval (s.t1))
; S nonterminal
(struct Struct_$pass ())
(struct Struct_$throw ())
(struct Struct_$assign-x (e.t1))
(struct Struct_$assign-y (e.t1))
(struct Struct_$assign-c (e.t1))
(struct Struct_$cons (s.t1 s.t2))
(struct Struct_$while (b.t1 s.t1))
(struct Struct_$site (b.t1 s.t1 s.t2))
; E nonterminal
(struct Struct_$x ())
(struct Struct_$y ())
(struct Struct_$c ())
(struct Struct_$0 ())
(struct Struct_$1 ())
(struct Struct_$+ (e.t1 e.t2))
(struct Struct_$- (e.t1 e.t2))
(struct Struct_$ite (b.t1 e.t1 e.t2))
; B nonterminal
(struct Struct_$true ())
(struct Struct_$false ())
(struct Struct_$not (b.t1))
(struct Struct_$and (b.t1 b.t2))
(struct Struct_$lt (e.t1 e.t2))

;;; SYNTAX SECTION

(current-grammar-depth 3)
(define-grammar (gram)
  [Start
    (choose
      (Struct_$eval (S))
    )]
  [S
    (choose
      (Struct_$pass)
      (Struct_$throw)
      (Struct_$assign-x (E))
      (Struct_$assign-y (E))
      (Struct_$assign-c (E))
      (Struct_$cons (S) (S))
      (Struct_$while (B) (S))
      (Struct_$site (B) (S) (S))
    )]
  [E
    (choose
      (Struct_$x)
      (Struct_$y)
      (Struct_$c)
      (Struct_$0)
      (Struct_$1)
      (Struct_$+ (E) (E))
      (Struct_$- (E) (E))
      (Struct_$ite (B) (E) (E))
    )]
  [B
    (choose
      (Struct_$true)
      (Struct_$false)
      (Struct_$not (B))
      (Struct_$and (B) (B))
      (Struct_$lt (E) (E))
    )]
)

;;; SEMANTICS SECTION

(define (Start.Sem  start.t x0 y0)
  (destruct start.t
    [(Struct_$eval s.t1) (begin (define c0 0) (define-values ( x2 y2 c2) (S.Sem s.t1 x0 y0 c0)) c2)]
  )
)
(define (S.Sem  s.t x0 y0 c0)
  (destruct s.t
    [(Struct_$pass) (begin (values x0 y0 c0))]
    [(Struct_$throw) (begin (values x0 y0 c0))]
    [(Struct_$assign-x e.t1) (begin (define e.v1 (E.Sem e.t1 x0 y0 c0)) (values e.v1 y0 c0))]
    [(Struct_$assign-y e.t1) (begin (define e.v1 (E.Sem e.t1 x0 y0 c0)) (values x0 e.v1 c0))]
    [(Struct_$assign-c e.t1) (begin (define e.v1 (E.Sem e.t1 x0 y0 c0)) (values x0 y0 e.v1))]
    [(Struct_$cons s.t1 s.t2) (begin (define-values ( x1 y1 c1) (S.Sem s.t1 x0 y0 c0)) (define-values ( x2 y2 c2) (S.Sem s.t2 x1 y1 c1)) (values x2 y2 c2))]
    [(Struct_$while b.t1 s.t1) (begin (define b.v1 (B.Sem b.t1 x0 y0 c0)) (if (b.v1)
          (begin (define b.v1 (B.Sem b.t1 x0 y0 c0)) (define-values ( x1 y1 c1) (S.Sem s.t1 x0 y0 c0)) (define-values ( x2 y2 c2) (S.Sem s.t x1 y1 c1)) (values x2 y2 c2))
          (begin (define b.v1 (B.Sem b.t1 x0 y0 c0)) (values x0 y0 c0))))]
    [(Struct_$site b.t1 s.t1 s.t2) (begin (define b.v1 (B.Sem b.t1 x0 y0 c0)) (if (b.v1)
          (begin (define b.v1 (B.Sem b.t1 x0 y0 c0)) (define-values ( x2 y2 c2) (S.Sem s.t1 x0 y0 c0)) (values x2 y2 c2))
          (begin (define b.v1 (B.Sem b.t1 x0 y0 c0)) (define-values ( x2 y2 c2) (S.Sem s.t2 x0 y0 c0)) (values x2 y2 c2))))]
  )
)
(define (E.Sem  e.t x0 y0 c0)
  (destruct e.t
    [(Struct_$x)  x0]
    [(Struct_$y)  y0]
    [(Struct_$c)  c0]
    [(Struct_$0)  0]
    [(Struct_$1)  1]
    [(Struct_$+ e.t1 e.t2) (begin (define e.v1 (E.Sem e.t1 x0 y0 c0)) (define e.v2 (E.Sem e.t2 x0 y0 c0)) (+ e.v1 e.v2))]
    [(Struct_$- e.t1 e.t2) (begin (define e.v1 (E.Sem e.t1 x0 y0 c0)) (define e.v2 (E.Sem e.t2 x0 y0 c0)) (- e.v1 e.v2))]
    [(Struct_$ite b.t1 e.t1 e.t2) (begin (define b.v1 (B.Sem b.t1 x0 y0 c0)) (define e.v1 (E.Sem e.t1 x0 y0 c0)) (define e.v2 (E.Sem e.t2 x0 y0 c0)) (ite b.v1 e.v1 e.v2))]
  )
)
(define (B.Sem  b.t x0 y0 c0)
  (destruct b.t
    [(Struct_$true)  true]
    [(Struct_$false)  false]
    [(Struct_$not b.t1) (begin (define b.v1 (B.Sem b.t1 x0 y0 c0)) b.v1)]
    [(Struct_$and b.t1 b.t2) (begin (define b.v1 (B.Sem b.t1 x0 y0 c0)) (define b.v2 (B.Sem b.t2 x0 y0 c0)) (and b.v1 b.v2))]
    [(Struct_$lt e.t1 e.t2) (begin (define e.v1 (E.Sem e.t1 x0 y0 c0)) (define e.v2 (E.Sem e.t2 x0 y0 c0)) (< e.v1 e.v2))]
  )
)

;;; CONSTRAINTS SECTION

(define (sol) (gram))
(define sol_Start
(synthesize
#:forall (list)
#:guarantee (assert (and (equal? (Start.Sem (sol) 0 0) 0) (equal? (Start.Sem (sol) 0 1) 0) (equal? (Start.Sem (sol) 1 1) 1) (equal? (Start.Sem (sol) 3 5) 15) (equal? (Start.Sem (sol) 7 2) 14) (equal? (Start.Sem (sol) 5 8) 40)))))
(print-forms sol_Start)
