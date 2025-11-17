CREATE TABLE Suppliers (
    SupplierId INT AUTO_INCREMENT PRIMARY KEY, 
    SupplierCode VARCHAR(5) UNIQUE NOT NULL, 
    SupplierName VARCHAR(100) NOT NULL,
    Address VARCHAR(255),
    Province VARCHAR(50),
    City VARCHAR(50),
    PIC VARCHAR(100)
);

CREATE TABLE Orders (
    OrderId INT AUTO_INCREMENT PRIMARY KEY,
    OrderNo VARCHAR(10) UNIQUE NOT NULL,
    OrderDate DATE NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    SupplierCode VARCHAR(5) NOT NULL
);

INSERT INTO Suppliers (SupplierCode, SupplierName, Address, Province, City, PIC) VALUES
('AB', 'PT Aneka Besi', 'Jln Sudirman', 'DKI Jakarta', 'Jakarta Pusat', 'Mr. Amir'),
('DG', 'PT Diesel Guna', 'Jln Setiabudi', 'Jawa Barat', 'Bandung', 'Mrs Indah'),
('ST', 'PT Sempurna', 'Jln Jayakarta', 'DKI Jakarta', 'Jakarta Barat', 'Mr. Didi'),
('KL', 'PT Kriya Lestari', 'Jln Melati', 'Jawa Tengah', 'Solo', 'Mrs. Safitri'),
('MN', 'PT Multi Nusa', 'Jln Balikpapan', 'Sumatera Barat', 'Padang', 'Ms. Maria');

INSERT INTO Orders (OrderNo, OrderDate, SupplierCode, Amount) VALUES
('ORD-001', '2019-01-01', 'AB', 150000000.00),
('ORD-002', '2019-01-04', 'DG', 250000000.00),
('ORD-003', '2019-01-05', 'KL', 35000000.00),
('ORD-004', '2019-01-08', 'KL', 40000000.00),
('ORD-005', '2019-02-08', 'ST', 51000000.00),
('ORD-006', '2019-02-12', 'KL', 16500000.00),
('ORD-007', '2019-02-13', 'MN', 25000000.00),
('ORD-008', '2019-02-15', 'MN', 350000000.00),
('ORD-009', '2019-02-20', 'AB', 1200000000.00);

-- 1) Menampilkan total amount transaksi masing-masing kota
select s.City, sum(o.Amount) TotalAmount
  from Suppliers s 
  join Orders o on o.SupplierCode = s.SupplierCode
 group by 1
 order by 2 desc;

-- 2) Menampilkan total amount transaksi per Supplier untuk bulan Januari 2019 saja
select s.SupplierName, sum(o.Amount) TotalAmount
  from Suppliers s 
  join Orders o on o.SupplierCode = s.SupplierCode
 where DATE_FORMAT(o.OrderDate, '%m-%Y') = '01-2019'
 group by 1;

-- 3) Menampilkan tanggal transaksi terakhir dari masing masing supplier
select s.SupplierName, max(o.OrderDate) MaxOrderDate
  from Suppliers s 
  join Orders o on o.SupplierCode = s.SupplierCode
 group by 1;

-- 4) Menampilkan semua transaksi dari Supplier yang ada di provinsi Jawa Barat
-- dengan amount transaksinya di atas 30.000.000
select s.SupplierCode, s.SupplierName, o.OrderNo, o.OrderDate, o.Amount 
  from Suppliers s 
  join Orders o on o.SupplierCode = s.SupplierCode
 where s.Province = 'Jawa Barat'
   and o.Amount > 30000000;

-- 5) Menampilkan urutan supplier berdasarkan total amount transaksi
-- dari yang terbesar ke yang terkecil selama tahun 2019
select s.SupplierCode, s.SupplierName, sum(o.Amount) TotalAmount
  from Suppliers s 
  join Orders o on o.SupplierCode = s.SupplierCode
 where DATE_FORMAT(o.OrderDate, '%Y') = '2019'
 group by 1
 order by 3 desc, 1;