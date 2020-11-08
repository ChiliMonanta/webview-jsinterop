package com.example.webview;

import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.res.Configuration;
import android.graphics.Bitmap;
import android.graphics.Color;
import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.webkit.JavascriptInterface;
import android.webkit.WebChromeClient;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

public class MainActivity extends AppCompatActivity {

    WebView web;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        web = findViewById(R.id.webView1);
        web = new WebView(this);
        WebSettings ws = web.getSettings();
        ws.setJavaScriptEnabled(true);
        ws.setDomStorageEnabled(true);
        ws.setAllowContentAccess(true);
        ws.setAppCacheEnabled(true);
        ws.setUseWideViewPort(true);
        web.setBackgroundColor(Color.TRANSPARENT);
        web.addJavascriptInterface(new WebAppInterface(this), "Android");

        //web.loadUrl("http://10.0.2.2:5000"); // Emulator
        web.loadUrl("http://192.168.1.161:5000");
        web.setWebViewClient(new myWebClient());
        web.setWebChromeClient(new WebChromeClient());
        setContentView(web);
    }

    public class WebAppInterface {
        Context mContext;
        int counter = 1;

        /** Instantiate the interface and set the context */
        WebAppInterface(Context c) {
            mContext = c;
        }

        /** Show a toast from the web page */
        @JavascriptInterface
        public String showToast(String toast) {
            counter = counter * 2;
            Toast.makeText(mContext, toast, Toast.LENGTH_SHORT).show();
            Log.i("-->>", toast);
            return "ack " + counter;
        }

        /** Add x and y and return result */
        @JavascriptInterface
        public long add(int x, int y) {
            long sum = golib.Golib.add(x, y);
            Log.i("-->> ", x + " + " + y + " = " + sum);
            return sum;
        }

        /** Cosine of x and return result */
        @JavascriptInterface
        public double cosine(double x) {
            double radians = golib.Golib.cosine(x);
            Log.i("-->>", "cos(" + x + ") = " + radians);
            return radians;
        }

        /**
         *  Sort array of numbers
         *
         * NOTE: Slice is not supported by gomobile
         *
         * // skipped function Sort with unsupported parameter or return types
         *
         * @return []
         */
        @JavascriptInterface
        public String sort(long[] numbers) {
            Log.i("-->>", "Sort not implemented as slice are not supported by gomobile");
            Toast.makeText(mContext, "Slice not supported by gomobile.", Toast.LENGTH_SHORT).show();
            return "[]";
        }

        /** write to log with go lib */
        @JavascriptInterface
        public void golog(String msg) {
            double sum = golib.Golib.log(msg);
            Log.i("-->>", msg);
            Toast.makeText(mContext, "Sent to go log: " + msg, Toast.LENGTH_SHORT).show();
        }
    }

    public class myWebClient extends WebViewClient
    {
        @Override
        public void onPageStarted(WebView view, String url, Bitmap favicon) {
            super.onPageStarted(view, url, favicon);
        }

        @Override
        public boolean shouldOverrideUrlLoading(WebView view, String url) {
            view.loadUrl(url);
            return true;

        }

        @Override
        public void onPageFinished(WebView view, String url) {
            super.onPageFinished(view, url);


        }
    }

    //flip screen not loading again
    @Override
    public void onConfigurationChanged(Configuration newConfig){
        super.onConfigurationChanged(newConfig);
    }

    @Override
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        if(event.getAction() == KeyEvent.ACTION_DOWN){
            switch(keyCode)
            {
                case KeyEvent.KEYCODE_BACK:
                    if(web.canGoBack()){
                        web.goBack();
                    }
                    else
                    {
                        backButtonHandler();
                    }
                    return true;
            }

        }
        return super.onKeyDown(keyCode, event);
    }

    public void backButtonHandler() {
        AlertDialog.Builder alertDialog = new AlertDialog.Builder(
            MainActivity.this);

        alertDialog.setTitle("Your App Name");
        alertDialog.setIcon(R.drawable.ic_launcher_foreground);
        alertDialog.setMessage("Exit Now?");

        alertDialog.setPositiveButton("Exit",
            new DialogInterface.OnClickListener() {
                public void onClick(DialogInterface dialog, int which) {
                    finish();
                }
            });

        alertDialog.setNegativeButton("No",
            new DialogInterface.OnClickListener() {
                public void onClick(DialogInterface dialog, int which) {
                    dialog.cancel();
                }
            });
        alertDialog.show();
    }
}
