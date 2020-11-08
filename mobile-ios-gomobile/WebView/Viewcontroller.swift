import UIKit
import WebKit
import Golib

class ViewController: UIViewController, WKScriptMessageHandler, WKUIDelegate {

  @IBOutlet weak var webView: WKWebView!
  
  override func viewDidLoad() {
    super.viewDidLoad()
    let url = URL(string:"http://192.168.1.161:5000/")!
    let urlRequest = URLRequest(url: url)
    webView.configuration.userContentController.add(self, name: "jsHandler")
    webView.configuration.preferences.javaScriptEnabled = true
    webView.allowsBackForwardNavigationGestures = false
    webView.allowsLinkPreview = false
    webView.uiDelegate = self
    //webView.navigationDelegate = self
    webView.load(urlRequest)
  }
    
  func userContentController(_ userContentController: WKUserContentController, didReceive message: WKScriptMessage) {
    guard let body = message.body as? String else {
      return
    }
    do {
      let s = try JSONSerialization.jsonObject(with: body.data(using: .utf8)!, options: []) as? [String: Any]
      let action = s!["action"] as! String
      let msgId = s!["id"] as! String

      switch action {
      case "add":
        //let sum = add(x: s!["x"] as! Int, y: s!["y"] as! Int)
        let sum = GolibAdd(s!["x"] as! Int, s!["y"] as! Int)
        sendResponse(messageId: msgId, data: ["result": sum])
      case "cosine":
        let radians = cosine(x: s!["x"] as! Double)
        sendResponse(messageId: msgId, data: ["result": radians])
      case "sort":
        let sorted = sort()
        sendResponse(messageId: msgId, data: ["result": sorted])
      case "golog":
        golog(msg: s!["msg"] as! String)
        sendResponse(messageId: msgId, data: Dictionary<String, Any>())
      default:
        print("ERROR: Unknown action")
      }
    } catch {
        print("ops")
    }
  }
  
  func add(x: Int, y: Int) -> Int {
    let sum = x + y
    print("[swift] Add \(x) + \(y) = \(sum)")
    return sum
  }
  
  func cosine(x: Double) -> Double {
    let radians = GolibCosine(x)
    print("[swift] Cosine(\(x)) = \(radians)")
    return radians
  }
  
  func sort() -> [Int] {
    print("[swift] Sort not implemented as slice are not supported by gomobile")
    return []
  }
  
  func golog(msg: String) {
    let msgId = GolibLog(msg)
    print("[swift] log msgid \(msgId)")
  }

  func sendResponse(messageId: String, data: Dictionary<String, Any>) {
    let resp = ["action": "response", "id": messageId, "data": data] as [String : Any]

    let json = try? JSONSerialization.data(withJSONObject: resp)
    let jsonString =  String(data: json!, encoding: .utf8)
    let escapedJsonString = jsonString!.replacingOccurrences(of: "\"", with: "\\\"")
    webView!.evaluateJavaScript("receiveMessage(\"\(escapedJsonString)\");", completionHandler: nil)
  }

  func webView(_ webView: WKWebView, runJavaScriptAlertPanelWithMessage message: String, initiatedByFrame frame: WKFrameInfo, completionHandler: @escaping () -> Void) {
    let alertController = UIAlertController(title: message,message: nil,preferredStyle: .alert)
    alertController.addAction(UIAlertAction(title: "OK", style: .cancel) {_ in completionHandler()})
    self.present(alertController, animated: true, completion: nil)
  }
}

