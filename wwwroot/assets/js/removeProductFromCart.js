// Remove Product From Cart
const removeProductFromCart = async id => {
    let data
    if (typeof id === "number") {
        const res = await axios.get("/cart/removeproduct/" + id);
        data = res.data;
    } else {
        data = id;
    }

    const template = `
    <div class="cart-btn">
    <a href="/cart/index">
        <i class="icon ion-bag"></i>
        <span class="text">Cart :</span>
        <span class="total">$${data.reduce((partialSum, product) => partialSum + product.price * product.count + product.exTax, 0)}</span>
        <span class="count">${data.length}</span>
    </a>
</div>
<div class="mini-cart">
    <ul class="cart-items">
        ${data.map(item => `
            <li>
                <div class="single-cart-item d-flex">
                    <div class="cart-item-thumb">
                        <a href="/product/detail/${item.id}"><img src="/assets/images/product/${item.image}" alt="${item.name}" /></a>
                        <span class="product-quantity">${item.count}x</span>
                    </div>
                    <div class="cart-item-content media-body">
                        <h5 class="product-name"><a href="/product/detail/${item.id}">${item.name}</a></h5>  
                        <span class="product-price">€${item.price}</span>
                        <a class="product-close" onclick="removeProductFromCart(${item.id})"><i class="fal fa-times"></i></a>
                    </div>
                </div>
            </li>
            `

    ).join("")}
    </ul>
    <div class="price_content">
        <div class="cart-subtotals">
            <div class="products price_inline">
                <span class="label">Subtotal</span>
                <span class="value">€${data.reduce((partialSum, product) => partialSum + product.price * product.count, 0)}</span>
            </div>
            <div class="tax price_inline">
                <span class="label">Taxes</span>
                <span class="value">€${data.reduce((partialSum, product) => partialSum + product.exTax, 0)}</span>
            </div>
        </div>
        <div class="cart-total price_inline">
            <span class="label">Total</span>
            <span class="value">€${data.reduce((partialSum, product) => partialSum + product.price * product.count + product.exTax, 0)}</span>
        </div>
    </div> <!-- price content -->
    <div class="checkout text-center">
        <a href="checkout.html" class="main-btn">Checkout</a>
    </div>
</div>
`

    document.querySelectorAll(".header-cart").forEach(cart => cart.innerHTML = template);
}