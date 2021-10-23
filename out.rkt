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

;;; STRUCT DECL SECTION

; E nonterminal
(struct Struct_$x ())
(struct Struct_$y ())
(struct Struct_$0 ())
(struct Struct_$1 ())
(struct Struct_$+ (et1 et2))
(struct Struct_$ite (bt1 et1 et2))
; B nonterminal
(struct Struct_$true ())
(struct Struct_$false ())
(struct Struct_$not (bt1))
(struct Struct_$and (bt1 bt2))
(struct Struct_$lt (et1 et2))

;;; SYNTAX SECTION

(current-grammar-depth 4)
(define-grammar (gram)
  [E
    (choose
      (Struct_$x)
      (Struct_$y)
      (Struct_$0)
      (Struct_$1)
      (Struct_$+ (E) (E))
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
(
  (define (E.Sem p x y)
    (destruct p
      [(struct Struct_$x) x]
      [(struct Struct_$y) y]
      [(struct Struct_$0) 0]
      [(struct Struct_$1) 1]
      [(struct Struct_$+ et1 et2) (begin (define iv1 (E.Sem et1 x y)) (define iv2 (E.Sem et2 x y)) (+ iv1 iv2))]
      [(struct Struct_$ite bt1 et1 et2) (begin (define bv1 (B.Sem bt1 x y)) (define iv1 (E.Sem et1 x y)) (define iv2 (E.Sem et2 x y)) (ite bv1 iv1 iv2))]))
  (define (B.Sem p x y)
    (destruct p
      [(struct Struct_$true) true]
      [(struct Struct_$false) false]
      [(struct Struct_$not bt1) (begin (define bv1 (B.Sem bt1 x y)) bv1)]
      [(struct Struct_$and bt1 bt2) (begin (define bv1 (B.Sem bt1 x y)) (define bv2 (B.Sem bt2 x y)) (and bv1 bv2))]
      [(struct Struct_$lt et1 et2) (begin (define iv1 (E.Sem et1 x y)) (define iv2 (E.Sem et2 x y)) (< iv1 iv2))]))
)

;;; CONSTRAINTS SECTION

(define (sol) (gram))
(define sol_E
(synthesize
#:forall (list)
#:guarantee (assert(equal? (E.Sem (sol) 4 2) 4))))