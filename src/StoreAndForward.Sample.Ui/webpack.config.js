const path = require("path");
const webpack = require("webpack");
require("react-hot-loader/patch");
const ExtractTextPlugin = require("extract-text-webpack-plugin");
const CheckerPlugin = require("awesome-typescript-loader").CheckerPlugin;
const merge = require("webpack-merge");

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);
    const clientBundleOutputDir = "./wwwroot/dist";

    // Configuration in common to both client-side and server-side bundles
    const commonConfig = () => ({
        stats: { modules: false },
        resolve: {
            extensions: [".js", ".jsx", ".ts", ".tsx"],
        },
        output: {
            filename: "[name].js",
            //publicPath: "dist/" // Webpack dev middleware, if enabled, handles requests for this URL prefix
            publicPath: "./"
        },
        module: {
            rules: [
                { test: /\.tsx?$/, include: /ClientApp/, use: "awesome-typescript-loader" },
                { test: /\.(png|jpg|jpeg|gif|svg)$/, use: "url-loader?limit=25000" },
            ]
        },
        plugins: [new CheckerPlugin()]
    });

    // Configuration for client-side bundle suitable for running in browsers

    const clientBundleConfig = merge(commonConfig(),
        {
            entry: { 'main-client': "./ClientApp/boot-client.tsx" },
            
            module: {
                rules: [
                    {
                        test: /\.(jpe?g|png|gif)$/i,
                        loader: "file-loader",
                        query: {
                            name: '[name].[ext]',
                            outputPath: 'images/'
                        }
                    },
                    {
                        test: /\.(woff(2)?|ttf|eot|svg)(\?v=[0-9]\.[0-9]\.[0-9])?$/,
                        loader: "url-loader",
                        query: {
                            limit: '10000',
                            name: '[name].[ext]',
                            outputPath: 'fonts/'
                        }
                    },
                    {
                        test: /\.(css|scss)?$/,
                        use: ExtractTextPlugin.extract({
                            fallback: "style-loader",
                            use: ['css-loader', 'sass-loader']
                        })
                    }
                ]
            },
            output: {
                path: path.join(__dirname, clientBundleOutputDir)
            },
            plugins:
            [
                new webpack.ProvidePlugin(
                    {
                        $: "jquery",
                        jQuery: "jquery",
                        'window.jQuery': 'jquery',
                        Popper: ['popper.js', 'default'],
                        //Util: "exports-loader?Util!bootstrap/js/dist/util",
                        //Dropdown: "exports-loader?Dropdown!bootstrap/js/dist/dropdown",
                    }
                    
                ),
                new ExtractTextPlugin("site.css"),
                new webpack.DllReferencePlugin({
                    context: __dirname,
                    manifest: require("./wwwroot/dist/vendor-manifest.json")
                })
            ].concat(isDevBuild ? [
                // Plugins that apply in development builds only
                new webpack.SourceMapDevToolPlugin({
                    filename: "[file].map", // Remove this line if you prefer inline source maps
                    moduleFilenameTemplate:
                    path.relative(clientBundleOutputDir,
                        "[resourcePath]") // Point sourcemap entries to the original file locations on disk
                })] : [
                    // Plugins that apply in production builds only
                    new webpack.optimize.UglifyJsPlugin()
                ])
        });

    // Configuration for server-side (prerendering) bundle suitable for running in Node
    const serverBundleConfig = merge(commonConfig(),
        {
            resolve: { mainFields: ["main"] },
            entry: { 'main-server': "./ClientApp/boot-server.tsx" },
            plugins: [
                new webpack.DllReferencePlugin({
                    context: __dirname,
                    manifest: require("./ClientApp/dist/vendor-manifest.json"),
                    sourceType: "commonjs2",
                    name: "./vendor"
                })
            ],
            output: {
                libraryTarget: "commonjs",
                path: path.join(__dirname, "./ClientApp/dist")
            },
            target: "node",
            devtool: "inline-source-map"
        });

    return [clientBundleConfig, serverBundleConfig];
};