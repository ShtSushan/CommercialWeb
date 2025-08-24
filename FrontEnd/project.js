
        // Global variables
        const API_BASE = 'https://localhost:7250/api'; // Update this to your API URL
        let products = [];
        let cart = JSON.parse(localStorage.getItem('cart')) || [];
        let currentUser = JSON.parse(localStorage.getItem('currentUser')) || null;
        let currentFilter = 'all';

        // Initialize the app
        document.addEventListener('DOMContentLoaded', function() {
            loadProducts();
            updateCartUI();
            updateCartCount();
        });

        // Navigation
        function showSection(sectionName) {
            document.querySelectorAll('.section').forEach(section => {
                section.classList.remove('active');
            });
            document.getElementById(sectionName).classList.add('active');
            
            if (sectionName === 'checkout') {
                updateCheckoutUI();
            }
        }

        // Products
        async function loadProducts() {
            try {
                const response = await fetch(`${API_BASE}/products`);
                products = await response.json();
                displayProducts(products);
            } catch (error) {
                console.error('Error loading products:', error);
                // Fallback to demo data if API is not available
                products = [
                    { id: 1, name: "Smartphone", description: "Latest model smartphone", price: 699.99, imageUrl: "https://via.placeholder.com/200x200?text=Phone", stock: 50, category: "Electronics" },
                    { id: 2, name: "Laptop", description: "High-performance laptop", price: 999.99, imageUrl: "https://via.placeholder.com/200x200?text=Laptop", stock: 30, category: "Electronics" },
                    { id: 3, name: "T-Shirt", description: "Cotton T-shirt", price: 19.99, imageUrl: "https://via.placeholder.com/200x200?text=T-Shirt", stock: 100, category: "Clothing" },
                    { id: 4, name: "Jeans", description: "Blue jeans", price: 49.99, imageUrl: "https://via.placeholder.com/200x200?text=Jeans", stock: 75, category: "Clothing" },
                    { id: 5, name: "Headphones", description: "Wireless headphones", price: 199.99, imageUrl: "https://via.placeholder.com/200x200?text=Headphones", stock: 40, category: "Electronics" },
                    { id: 6, name: "Sneakers", description: "Running sneakers", price: 79.99, imageUrl: "https://via.placeholder.com/200x200?text=Sneakers", stock: 60, category: "Footwear" }
                ];
                displayProducts(products);
            }
        }

        function displayProducts(productsToShow) {
            const grid = document.getElementById('productsGrid');
            grid.innerHTML = productsToShow.map(product => `
                <div class="product-card">
                    <img src="${product.imageUrl}" alt="${product.name}" class="product-image">
                    <h3 class="product-name">${product.name}</h3>
                    <p class="product-description">${product.description}</p>
                    <div class="product-price">$${product.price.toFixed(2)}</div>
                    <button class="btn btn-primary" onclick="addToCart(${product.id})">
                        Add to Cart
                    </button>
                </div>
            `).join('');
        }

        function filterProducts(category) {
            currentFilter = category;
            
            // Update active filter button
            document.querySelectorAll('.filter-btn').forEach(btn => {
                btn.classList.remove('active');
            });
            event.target.classList.add('active');
            
            // Filter products
            const filtered = category === 'all' 
                ? products 
                : products.filter(p => p.category === category);
            
            displayProducts(filtered);
        }

        // Cart functionality
        function addToCart(productId) {
            const product = products.find(p => p.id === productId);
            if (!product) return;

            const existingItem = cart.find(item => item.id === productId);
            if (existingItem) {
                existingItem.quantity += 1;
            } else {
                cart.push({
                    id: product.id,
                    name: product.name,
                    price: product.price,
                    imageUrl: product.imageUrl,
                    quantity: 1
                });
            }

            localStorage.setItem('cart', JSON.stringify(cart));
            updateCartCount();
            
            // Show success message
            alert(`${product.name} added to cart!`);
        }

        function removeFromCart(productId) {
            cart = cart.filter(item => item.id !== productId);
            localStorage.setItem('cart', JSON.stringify(cart));
            updateCartUI();
            updateCartCount();
        }

        function updateQuantity(productId, newQuantity) {
            if (newQuantity <= 0) {
                removeFromCart(productId);
                return;
            }

            const item = cart.find(item => item.id === productId);
            if (item) {
                item.quantity = newQuantity;
                localStorage.setItem('cart', JSON.stringify(cart));
                updateCartUI();
                updateCartCount();
            }
        }

        function updateCartCount() {
            const count = cart.reduce((total, item) => total + item.quantity, 0);
            document.getElementById('cartCount').textContent = count;
        }

        function updateCartUI() {
            const cartItemsContainer = document.getElementById('cartItems');
            const cartTotal = document.getElementById('cartTotal');
            const checkoutBtn = document.getElementById('checkoutBtn');

            if (cart.length === 0) {
                cartItemsContainer.innerHTML = `
                    <div class="empty-cart">
                        <h3>Your cart is empty</h3>
                        <p>Add some products to get started!</p>
                        <button class="btn btn-primary" onclick="showSection('products')">
                            Continue Shopping
                        </button>
                    </div>
                `;
                cartTotal.style.display = 'none';
                checkoutBtn.style.display = 'none';
                return;
            }

            cartItemsContainer.innerHTML = cart.map(item => `
                <div class="cart-item">
                    <img src="${item.imageUrl}" alt="${item.name}">
                    <div class="cart-item-info">
                        <div class="cart-item-name">${item.name}</div>
                        <div class="cart-item-price">${item.price.toFixed(2)}</div>
                    </div>
                    <div class="quantity-controls">
                        <button class="quantity-btn" onclick="updateQuantity(${item.id}, ${item.quantity - 1})">-</button>
                        <span>${item.quantity}</span>
                        <button class="quantity-btn" onclick="updateQuantity(${item.id}, ${item.quantity + 1})">+</button>
                    </div>
                    <button class="btn btn-danger" onclick="removeFromCart(${item.id})">Remove</button>
                </div>
            `).join('');

            const total = cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
            cartTotal.textContent = `Total: ${total.toFixed(2)}`;
            cartTotal.style.display = 'block';
            checkoutBtn.style.display = 'block';
        }

        function checkout() {
            if (cart.length === 0) {
                alert('Your cart is empty!');
                return;
            }
            showSection('checkout');
        }

        function updateCheckoutUI() {
            const total = cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
            document.getElementById('checkoutTotal').textContent = `Total: ${total.toFixed(2)}`;
            
            if (currentUser) {
                document.getElementById('checkoutName').value = currentUser.name;
                document.getElementById('checkoutEmail').value = currentUser.email;
                document.getElementById('checkoutAddress').value = currentUser.address || '';
            }
        }

        // User Authentication
        async function register(event) {
            event.preventDefault();
            
            const userData = {
                name: document.getElementById('registerName').value,
                email: document.getElementById('registerEmail').value,
                password: document.getElementById('registerPassword').value,
                address: document.getElementById('registerAddress').value
            };

            try {
                const response = await fetch(`${API_BASE}/users/register`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(userData)
                });

                if (response.ok) {
                    const user = await response.json();
                    document.getElementById('registerMessage').innerHTML = 
                        '<div class="success-message">Registration successful! You can now login.</div>';
                    document.querySelector('#register form').reset();
                } else {
                    throw new Error('Registration failed');
                }
            } catch (error) {
                document.getElementById('registerMessage').innerHTML = 
                    '<div style="color: red; margin-top: 1rem;">Registration failed. Please try again.</div>';
            }
        }

        async function login(event) {
            event.preventDefault();
            
            const loginData = {
                email: document.getElementById('loginEmail').value,
                password: document.getElementById('loginPassword').value
            };

            try {
                const response = await fetch(`${API_BASE}/users/login`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(loginData)
                });

                if (response.ok) {
                    const user = await response.json();
                    currentUser = user;
                    localStorage.setItem('currentUser', JSON.stringify(user));
                    
                    document.getElementById('loginMessage').innerHTML = 
                        '<div class="success-message">Login successful! Welcome back!</div>';
                    
                    // Update navigation to show user name
                    setTimeout(() => {
                        showSection('products');
                        updateNavigation();
                    }, 1500);
                } else {
                    throw new Error('Login failed');
                }
            } catch (error) {
                document.getElementById('loginMessage').innerHTML = 
                    '<div style="color: red; margin-top: 1rem;">Invalid email or password.</div>';
            }
        }

        function updateNavigation() {
            if (currentUser) {
                const navLinks = document.querySelector('.nav-links');
                navLinks.innerHTML = `
                    <li><a href="#" onclick="showSection('products')">Products</a></li>
                    <li><a href="#" onclick="showSection('cart')">Cart</a></li>
                    <li><a href="#" onclick="showSection('orders')">My Orders</a></li>
                    <li><a href="#" onclick="logout()">Logout (${currentUser.name})</a></li>
                `;
            }
        }

        function logout() {
            currentUser = null;
            localStorage.removeItem('currentUser');
            location.reload();
        }

        // Order functionality
        async function placeOrder(event) {
            event.preventDefault();
            
            if (cart.length === 0) {
                alert('Your cart is empty!');
                return;
            }

            const orderData = {
                userId: currentUser ? currentUser.id : 0,
                userName: document.getElementById('checkoutName').value,
                userEmail: document.getElementById('checkoutEmail').value,
                shippingAddress: document.getElementById('checkoutAddress').value,
                items: cart.map(item => ({
                    productId: item.id,
                    productName: item.name,
                    price: item.price,
                    quantity: item.quantity,
                    total: item.price * item.quantity
                })),
                totalAmount: cart.reduce((sum, item) => sum + (item.price * item.quantity), 0)
            };

            try {
                const response = await fetch(`${API_BASE}/orders`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(orderData)
                });

                if (response.ok) {
                    const order = await response.json();
                    
                    // Clear cart
                    cart = [];
                    localStorage.removeItem('cart');
                    updateCartCount();
                    
                    // Show success message
                    document.getElementById('checkoutMessage').innerHTML = 
                        `<div class="success-message">
                            Order placed successfully! 
                            <br>Order ID: ${order.id}
                            <br>Total: ${order.totalAmount.toFixed(2)}
                        </div>`;
                    
                    // Reset form
                    document.querySelector('#checkout form').reset();
                    
                    // Redirect to products after 3 seconds
                    setTimeout(() => {
                        showSection('products');
                    }, 3000);
                    
                } else {
                    throw new Error('Order failed');
                }
            } catch (error) {
                document.getElementById('checkoutMessage').innerHTML = 
                    '<div style="color: red; margin-top: 1rem;">Order failed. Please try again.</div>';
            }
        }

        // Initialize user state
        if (currentUser) {
            updateNavigation();
        }
   