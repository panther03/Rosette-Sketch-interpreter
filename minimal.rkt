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
(struct Struct_asnx (e.t1))
(struct Struct_asny (e.t1))
(struct Struct_$assign-c (e.t1))
(struct Struct_$cons (s.t1 s.t2))
(struct Struct_$site (b.t1 s.t1 s.t2))
; E nonterminal
(struct Struct_$x ())
(struct Struct_$y ())
(struct Struct_$c ())
(struct Struct_zero ())
(struct Struct_$1 ())
(struct Struct_$+ (e.t1 e.t2))
(struct Struct_$- (e.t1 e.t2))
(struct Struct_$ite (b.t1 e.t1 e.t2))

;;; SYNTAX SECTION

(current-grammar-depth 3)
(define-grammar (gram)
  [S
    (choose
      (Struct_$pass)
     (Struct_$throw)
 ;     (Struct_asnx (E))
   ;   (Struct_asny (E))
;      (Struct_$cons (S) (S))
;      (Struct_$site (B) (S) (S))
    )]
  [E
    (choose
   ;  (Struct_$x)
   ;  (Struct_$y)
   ;  (Struct_$c)
     (Struct_zero)
     (Struct_$1)
   ;  (Struct_$+ (E) (E))
   ;  (Struct_$- (E) (E))
   ;  (Struct_$ite (B) (E) (E))
    )]
)

;;; SEMANTICS SECTION

(define (S.Sem  s.t x0 y0 c0)
  (destruct s.t
    [(Struct_asnx e.t1) (values (E.Sem e.t1 x0 y0 c0) y0 c0)]
    [(Struct_$pass) (begin (values x0 y0 c0))]
    [(Struct_$throw) (begin (values x0 y0 c0))]
  )
)
(define (E.Sem  e.t x0 y0 c0)
  (destruct e.t
    [(Struct_zero)  0]
    [(Struct_$1) 1]
  )
)

;;; CONSTRAINTS SECTION

(define (sol) (gram))
(define sol_Start
(synthesize
#:forall (list)
#:guarantee (assert (equal? (let-values ([(a b c) (S.Sem (sol) 1 1 1)]) (list a b c)) (list 1 1 1)))))

(define prog (Struct_asnx (Struct_zero)))
(equal? (let-values ([(a b c) (S.Sem (Struct_asnx (Struct_$1)) 1 0 1)]) (list a b c)) (list 1 1 1))
(print-forms sol_Start)
