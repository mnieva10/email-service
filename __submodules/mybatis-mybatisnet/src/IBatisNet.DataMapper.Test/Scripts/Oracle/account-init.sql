DROP TABLE ACCOUNTS CASCADE CONSTRAINTS;

CREATE TABLE ACCOUNTS  (
   ACCOUNT_ID				INTEGER                          NOT NULL,
   ACCOUNT_FIRSTNAME		VARCHAR2(32)                     NOT NULL,
   ACCOUNT_LASTNAME			VARCHAR2(32)                     NOT NULL,
   ACCOUNT_EMAIL			VARCHAR2(128),
   ACCOUNT_BANNER_OPTION	VARCHAR2(255),
   ACCOUNT_CART_OPTION		INTEGER,   
   CONSTRAINT PK_ACCOUNTS PRIMARY KEY (ACCOUNT_ID)
)
NOLOGGING 
NOCACHE 
NOPARALLEL;

INSERT INTO ACCOUNTS VALUES(1,'Joe', 'Dalton', 'Joe.Dalton@somewhere.com', 'Oui', 200);
INSERT INTO ACCOUNTS VALUES(2,'Averel', 'Dalton', 'Averel.Dalton@somewhere.com', 'Oui', 200);
INSERT INTO ACCOUNTS VALUES(3,'William', 'Dalton', null, 'Non', 100);
INSERT INTO ACCOUNTS VALUES(4,'Jack', 'Dalton', 'Jack.Dalton@somewhere.com', 'Non', 100);
INSERT INTO ACCOUNTS VALUES(5,'Gilles', 'Bayon', null, 'Oui', 100);